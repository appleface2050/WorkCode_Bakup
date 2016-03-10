<?php
/**
 * 活动相关
 *
 */
class ActivityController extends RestController
{

    /**
     * 获取活动详情
     */
    public function actionGetActivity()
    {
        $this->success(array(
            'activity_name' => '',
            'activity_url'  => '',
        ));
        /*$this->success(array(
            'activity_name' => '5年免费更换滤芯',
            'activity_url'  => Yii::app()->request->hostInfo .'/change_filter/exchange.html',
        ));*/
    }

    /**
     * 判断是否有新活动
     */
    public function actionCheck()
    {
        $cleaner_id = Yii::app()->request->getParam('cleaner_id');
        if(!$cleaner_id)
            throw new CHttpException(400, '参数error！');
        //判断是否有管理权限
       /* if(!UserCleaner::model()->checkHasBind($this->getUser()->id,$cleaner_id))
            throw new CHttpException(400, '没有该净化器的管理权限');*/
        //判断是否可以参加活动
        $cleaner = CleanerStatus::model()->findByPk($cleaner_id);
        if(!$cleaner || !$cleaner->qrcode)
            throw new CHttpException(400, '无效的净化器');
        $free_exchange = CleanerFreeExchange::model()->findByAttributes(array('qrcode' => $cleaner->qrcode));
        if($free_exchange)
        {
            $activity = array(
                'activity_name' => $free_exchange->type_id == CleanerFreeExchange::TYPE_FREE_FIVE_YEARS ? '滤芯5年免费用' : '滤芯终身免费用',
                'activity_url'  => Yii::app()->request->hostInfo .'/change_filter/exchange.html',
            );
        }else
        {
            $activity = array(
                'activity_name' => '',
                'activity_url'  => '',
            );
        }
        $this->success($activity);
    }
}