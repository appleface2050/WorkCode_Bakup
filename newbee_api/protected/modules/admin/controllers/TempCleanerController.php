<?php
Yii::import('application.modules.admin.controllers.BaseController');
class TempCleanerController extends BaseController
{
	/**
	 * @var string the default layout for the views. Defaults to '//layouts/column2', meaning
	 * using two-column layout. See 'protected/views/layouts/column2.php'.
	 */
	//public $layout='column2';

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
			array('allow', // allow authenticated user to perform 'create' and 'update' actions
				'actions'=>array('create','index','view'),
				'users'=>array('@'),
			),
			array('allow', // allow admin user to perform 'admin' and 'delete' actions
				'actions'=>array('delete'),
				'users'=>array('admin'),
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
		$model=new CleanerStatus;

		// Uncomment the following line if AJAX validation is needed
		// $this->performAjaxValidation($model);

		if(isset($_POST['CleanerStatus']))
		{
			$model->attributes=$_POST['CleanerStatus'];
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

		if(isset($_POST['CleanerStatus']))
		{
			$model->attributes=$_POST['CleanerStatus'];
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
     * 清除临时sn
     * 清除临时的 只清除cleaenr_status 表
     * 清除临时的 只清除cleaenr_status 表
     */
	public function actionDelete($id)
	{
		if(Yii::app()->request->isPostRequest)
		{
			// we only allow deletion via POST request
			//$this->loadModel($id)->delete();

            //update cleaner_status set qrcode='' where qrcode !='' and char_length(qrcode)<9
            $model = $this->loadModel($id);
            $model->updateByPk($id,array('qrcode'=>''));
            //清空cleaner表中的cleaner_id
            //Cleaner::model()->updateByPk($model->qrcode,array('cleaner_id'=>''));
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
		$model=new CleanerStatus();
		$model->unsetAttributes();  // clear any default values
		if(isset($_GET['CleanerStatus']))
			$model->attributes=$_GET['CleanerStatus'];

        $criteria=new CDbCriteria;
        $criteria->condition = " qrcode !='' and char_length(qrcode)<9 ";
        if($model->id)
            $criteria->condition.=" AND id = '{$model->id}'";
        if($model->qrcode)
            $criteria->condition.=" AND qrcode = '{$model->qrcode}' ";

        $dataProvider = new CActiveDataProvider('CleanerStatus', array(
            'criteria'=>$criteria,
        ));

		$this->render('index',array(
			'dataProvider'=>$dataProvider,
            'model' => $model
		));
	}

	/**
	 * Returns the data model based on the primary key given in the GET variable.
	 * If the data model is not found, an HTTP exception will be raised.
	 * @param integer the ID of the model to be loaded
	 */
	public function loadModel($id)
	{
		$model=CleanerStatus::model()->findByPk($id);
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
		if(isset($_POST['ajax']) && $_POST['ajax']==='cleaner-status-form')
		{
			echo CActiveForm::validate($model);
			Yii::app()->end();
		}
	}
}
