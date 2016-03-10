<?php

Yii::import('application.modules.admin.controllers.BaseController');

/**
 * 净化器
 * @author zhoujianjun
 *
 */
class CleanerController extends BaseController
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
			array('allow', // allow authenticated user to perform 'create' and 'update' actions
				'actions'=>array('index','view','update','SearchCity'),
				'users'=>array('@'),
			),
            array('allow', // allow authenticated user to perform 'create' and 'update' actions
                'actions'=>array('delete','UpdateCity','clearBind','BatchClear'),
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
        $model = $this->loadModel($id);
        $surplusStr = '';
        $surplusArr = CleanerStatus::countSurplusLife($model->filter_surplus_life,$model->type,$model->version);
        if($surplusArr)
        {
            foreach($surplusArr as $surplus)
            {
                $surplusStr.= $surplus['name'].':['.$surplus['surplus_life'].'] ['.$surplus['life'].'],';
            }
        }
        $model->filter_surplus_life = $surplusStr;
		$this->render('view',array(
			'model'=>$model,
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

        $tpl = $id ? 'update_city' : 'update';
		$this->render($tpl,array(
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
		$model=new CleanerStatus('search');
		$model->unsetAttributes();  // clear any default values
		if(isset($_GET['CleanerStatus']))
			$model->attributes=$_GET['CleanerStatus'];

		$this->render('index',array(
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

    /**
     * 获取城市
     */
    public function actionSearchCity()
    {
        $sql = "SELECT city FROM ".AqiIndex::model()->tableName()." WHERE city like '%".trim($_GET['q'])."%'";
        $connection=Yii::app()->db;
        $result = $connection->createCommand($sql)->queryAll();
        $city = '';
        if($result)
        {
            foreach($result as $val)
            {
                $city.= $val['city']."|\n";
            }
        }
        echo $city;
        die;
    }

    /**
     * 更新净化器所在城市
     * @param $id
     * @throws CHttpException
     */
    public function actionUpdateCity($id)
    {
        $model=$this->loadModel($id);

        // Uncomment the following line if AJAX validation is needed
        // $this->performAjaxValidation($model);

        if(isset($_POST['CleanerStatus']))
        {
            $city = trim($_POST['CleanerStatus']['city']);
            $city = str_replace('市','',$city);
            if($city && !AqiIndex::model()->findByPk($city))
            {
                throw new CHttpException(400,'无效的城市（系统里没有该城市的pm2.5数据）');
            }
            if($city == $model->city || $model->updateByPk($id,array('city'=>$city)))
                $this->redirect(array('view','id'=>$model->id));
        }
        $tpl = 'update_city';
        $this->render($tpl,array(
            'model'=>$model,
        ));
    }

    /**
     * 解除绑定
     * @param $id
     * @throws CHttpException
     */
    public function actionClearBind($id)
    {
        if(Yii::app()->request->isPostRequest)
        {
            // we only allow deletion via POST request
            $model = $this->loadModel($id);
            //$model->updateByPk($id,array('qrcode'=>''));
			$model->delete();
            Cleaner::model()->updateByPk($model->qrcode,array('cleaner_id'=>''));
			//删除用户绑定
			UserCleaner::model()->deleteAllByAttributes(array('cleaner_id' => $id));

            // if AJAX request (triggered by deletion via admin grid view), we should not redirect the browser
            if(!isset($_GET['ajax']))
                $this->redirect(isset($_POST['returnUrl']) ? $_POST['returnUrl'] : array('admin'));
        }
        else
            throw new CHttpException(400,'Invalid request. Please do not repeat this request again.');
    }

    /**
     * 批量解除绑定
     * @param $id
     */
    public function actionBatchClear()
    {
        $data = array('error' => '','success' => '');
        if(isset($_POST['qrcodes']))
        {
            $qrcodes = trim($_POST['qrcodes']);
            if(!$qrcodes)
            {
                $data['error'] = 'SN不能为空';
            }else
            {
                $qrcode_arr = explode("\n",$qrcodes);
                $sn = '';
                foreach($qrcode_arr as $s)
                {
                    $s = trim($s);
                    if($s)
                        $sn.=" '{$s}',";
                }
                $sn =trim($sn,',');
                //执行sql处理
                $sql = ' update cleaner set cleaner_id = "" where qrcode in ('.$sn.')';
                //echo $sql.'<br>';
                $res = Yii::app()->db->createCommand($sql)->execute();
                
                $sql = ' delete from user_cleaner_rel where cleaner_id in (select id from cleaner_status where qrcode in ('.$sn.'))';
                //echo $sql.'<br>';
                Yii::app()->db->createCommand($sql)->execute();

                $sql = ' delete from  cleaner_status where qrcode in ('.$sn.')';
                //echo $sql.'<br>';
                Yii::app()->db->createCommand($sql)->execute();
                $data['success'] = '处理成功';
            }
        }
        $this->render('batch_clear',$data);
    }
}
