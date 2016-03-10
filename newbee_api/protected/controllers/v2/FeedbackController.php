<?php

/**
 * 问题反馈管理
 * @author zhoujianjun
 *
 */
class FeedbackController extends RestController
{
	
	
	/**
	 * 我的反馈列表
	 */
	public function actionIndex()
	{
		$result = Feedback::model()->findAllByAttributes(array('user_id'=>Yii::app()->user->id));
		$data = array();
		if (!empty($result))
		{
			foreach ($result as $value)
			{
				$data[] = array(
					'type' => $value->type,
					'detail' => $value->detail,
					'add_time' => date('Y-m-d H:i:s', $value->add_time),
					'status' => intval($value->status)
				);
			}
		}
		$this->success($data);
	}
	
	/**
	 * 添加故障报告
	 */
	public function actionCreate()
	{
		$type = intval(Yii::app()->request->getParam('type'));
		$detail = Yii::app()->request->getParam('detail');
		$model = new Feedback();
		$model->attributes = array(
			'user_id' => Yii::app()->user->id,
			'type' => $type,
			'detail' => $detail,
		);
		if ($model->save())
		{
			$this->success();
		} else
		{
			$error = array_values($model->getErrors());
			$this->failed($error[0][0]);
		}
	}
	
	
}