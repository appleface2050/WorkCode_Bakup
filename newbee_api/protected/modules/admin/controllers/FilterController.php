<?php
Yii::import('application.modules.admin.controllers.BaseController');
class FilterController extends BaseController
{

	/**
	 * Updates a particular model.
	 * If update is successful, the browser will be redirected to the 'view' page.
	 * @param integer $id the ID of the model to be updated
	 */
	public function actionUpdate($id)
	{
		$model=$this->loadModel($id);
		//老高达 2.1.0 及以上，
        $allow_cleaner = array(
            CleanerStatus::TYPE_BIG_WITHOUT => '2.1.0',
            CleanerStatus::TYPE_GAODA_NEW   => '3.0.0',
            CleanerStatus::TYPE_SHOUHU_SECOND   => '2.0.0'
        );
        if(!isset($allow_cleaner[$model->type]))
            throw new CHttpException(400,'该净化器不支持设置滤芯寿命');
        if($model->version < $allow_cleaner[$model->type])
            throw new CHttpException(400,'净化器当前固件版本为'.$model->version.',不支持设置滤芯寿命');

		//判断是否在线
		$key = EKeys::getAllonlineKey();
		if(!Yii::app()->redis->sIsMember($key, $id))
			throw new CHttpException(400,'净化器不在线，不能设置滤芯寿命');

		// Uncomment the following line if AJAX validation is needed
		// $this->performAjaxValidation($model);

		if(isset($_POST['CleanerStatus']))
		{
            $filter_id = intval(Yii::app()->request->getParam('filter_id'));
            if(!$filter_id)
                throw new CHttpException(400,'无效的滤芯id');
            $life_value = intval($_POST['life_value']);
            if(!$life_value)
                throw new CHttpException(400,'请输入滤芯寿命值');

            $default_life = CleanerStatus::model()->getDefaultLife($model->type, $filter_id,$model->version);
            if(!$default_life)
                throw new CHttpException(404, '无效的滤芯');

            if($life_value > $default_life)
                throw new CHttpException(404, '滤芯寿命值不能大于'.$default_life);

            $filter = json_decode($model->filter_surplus_life, true);
            $filter[$filter_id] = $life_value;
            $attributes = array('filter_surplus_life' => json_encode($filter), 'update_time'=>time());
            $model->saveAttributes($attributes);

            $key = EKeys::getCleanerCacheKey($model->id);
            $message = $model->id . '-' . CleanerStatus::OP_CODE_MODIFY_FILTER;

			$life_length = 6;
			if($model->type == CleanerStatus::TYPE_GAODA_NEW)
			{
				$life_length = 8;
			}
            Yii::app()->redis->hset($key,'modify_filter', $filter_id.'^'.str_pad($life_value,$life_length,"0",STR_PAD_LEFT));
            Yii::app()->publish->publishParameter($message);
            $this->redirect(array('cleaner/view','id'=>$model->id));
		}
        $life = CleanerStatus::countSurplusLife($model->filter_surplus_life,$model->type);
		$this->render('update',array(
			'model'=>$model,
            'life' => $life
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
		if(isset($_POST['ajax']) && $_POST['ajax']==='cleaner-free-exchange-form')
		{
			echo CActiveForm::validate($model);
			Yii::app()->end();
		}
	}
}
