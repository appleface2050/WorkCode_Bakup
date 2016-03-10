<?php

Yii::import('application.modules.admin.controllers.BaseController');

/**
 * 在线净化器相关统计
 * @author zhoujianjun
 */


class OnlineController extends BaseController
{
	/**
	 * @var string the default layout for the views. Defaults to '//layouts/column2', meaning
	 * using two-column layout. See 'protected/views/layouts/column2.php'.
	 */
	public $layout='column1';

	/**
	 * @return array action filters
	 */
	public function filters()
	{
		return array(
			'accessControl', // perform access control for CRUD operations
		);
	}

	/**
	 * Specifies the access control rules.
	 * This method is used by the 'accessControl' filter.
	 * @return array access control rules
	 */
	public function accessRules()
	{
		return array(
			array('allow',  // allow all users to perform 'index' and 'view' actions
				'actions'=>array('index', 'date','online'),
				'users'=>array('@'),
			),
			array('deny',  // deny all users
				'users'=>array('*'),
			),
		);
	}

	/**
	 * Displays a particular model.
	 * @param integer $id the ID of the model to be displayed
	 */
	public function actionView($id)
	{
		$this->render('view',array(
			'model'=>$this->loadModel($id),
		));
	}

	/**
	 * Creates a new model.
	 * If creation is successful, the browser will be redirected to the 'view' page.
	 */
	public function actionCreate()
	{
		$model=new OnlineCleanerCount;

		// Uncomment the following line if AJAX validation is needed
		// $this->performAjaxValidation($model);

		if(isset($_POST['OnlineCleanerCount']))
		{
			$model->attributes=$_POST['OnlineCleanerCount'];
			if($model->save())
				$this->redirect(array('view','id'=>$model->id));
		}

		$this->render('create',array(
			'model'=>$model,
		));
	}

	/**
	 * Updates a particular model.
	 * If update is successful, the browser will be redirected to the 'view' page.
	 * @param integer $id the ID of the model to be updated
	 */
	public function actionUpdate($id)
	{
		$model=$this->loadModel($id);

		// Uncomment the following line if AJAX validation is needed
		// $this->performAjaxValidation($model);

		if(isset($_POST['OnlineCleanerCount']))
		{
			$model->attributes=$_POST['OnlineCleanerCount'];
			if($model->save())
				$this->redirect(array('view','id'=>$model->id));
		}

		$this->render('update',array(
			'model'=>$model,
		));
	}

	/**
	 * Deletes a particular model.
	 * If deletion is successful, the browser will be redirected to the 'admin' page.
	 * @param integer $id the ID of the model to be deleted
	 */
	public function actionDelete($id)
	{
		if(Yii::app()->request->isPostRequest)
		{
			// we only allow deletion via POST request
			$this->loadModel($id)->delete();

			// if AJAX request (triggered by deletion via admin grid view), we should not redirect the browser
			if(!isset($_GET['ajax']))
				$this->redirect(isset($_POST['returnUrl']) ? $_POST['returnUrl'] : array('admin'));
		}
		else
			throw new CHttpException(400,'Invalid request. Please do not repeat this request again.');
	}

	/**
	 * Lists all models.
	 */
	public function actionIndex()
	{
		$model=new OnlineCleanerCount('search');
		$model->unsetAttributes();  // clear any default values
		if(isset($_GET['OnlineCleanerCount']))
			$model->attributes=$_GET['OnlineCleanerCount'];

		$this->render('index',array(
			'model'=>$model,
		));
	}

	/**
	 * 某天在线过的全部净化器
	 */
	public function actionDate()
	{
		$date = Yii::app()->request->getParam('date');
		$model=new OnlineCleanerHistory('search');
		$model->unsetAttributes();  // clear any default values
		$model->attributes = array(
			'date' => $date
		);
		
		$this->render('history',array(
			'model'=>$model,
		));
		
	}
	
	
	/**
	 * Returns the data model based on the primary key given in the GET variable.
	 * If the data model is not found, an HTTP exception will be raised.
	 * @param integer the ID of the model to be loaded
	 */
	public function loadModel($id)
	{
		$model=OnlineCleanerCount::model()->findByPk($id);
		if($model===null)
			throw new CHttpException(404,'The requested page does not exist.');
		return $model;
	}

	/**
	 * Performs the AJAX validation.
	 * @param CModel the model to be validated
	 */
	protected function performAjaxValidation($model)
	{
		if(isset($_POST['ajax']) && $_POST['ajax']==='online-cleaner-count-form')
		{
			echo CActiveForm::validate($model);
			Yii::app()->end();
		}
	}


    public function actionOnline()
    {
        $online = Yii::app()->redis->sMembers(EKeys::getAllonlineKey());
      /*  if($online)
        {
            foreach($online as $cleaner_id)
            {
                echo $cleaner_id.'<br>';
            }
        }
        die;*/
        $data['online'] = $online;
        $this->render('online',array(
            'data'=>$data,
        ));
    }
}
