<?php
/**
 * 任务相关
 *
 */
class TaskController extends RestController
{
    public function init()
    {
        header("Access-Control-Allow-Origin:*");
        parent::init();
    }

    public function beforeAction($action)
    {
        //var_dump(Yii::app()->controller->action->id);die;
        $current_action = strtolower($this->getAction()->getId());
        if( in_array($current_action,array('support','getsupportuser')))
            return true;
        parent::beforeAction($action);
        return true;
    }
    /**
     * 登录给积分接口
     */
    public function actionLogin()
    {
        return;
        CreditLog::model()->doTodayTask($this->getUser()->id,CreditLog::ACTION_LOGIN);
        $this->success();
    }

    /**
     * 登录给积分接口
     */
    public function actionShare()
    {
        return;
        CreditLog::model()->doTodayTask($this->getUser()->id,CreditLog::ACTION_SHARE);
        $this->success();
    }

    /**
     * 获取滤芯信息
     */
    public function actionGetFilterInfo()
    {
        $cleaner_id = trim(Yii::app()->request->getParam('cleaner_id'));
        if(!$cleaner_id)
            throw new CHttpException(400, '参数错误');
        $cleaner = CleanerStatus::model()->findByPk($cleaner_id);
        if(!$cleaner)
            throw new CHttpException(401, '无效的净化器信息');
        list($type_id,$message) = CleanerFreeExchange::model()->checkCanJoinActivity($cleaner_id,$cleaner);
        if(!$type_id)
            throw new CHttpException(400, $message);
        //判断该用户是否有权限操作该净化器
        $hasBind = UserCleaner::model()->checkHasBind($this->getUser()->id,$cleaner_id);
        if(!$hasBind)
            throw new CHttpException(400, '没有权限查看该净化器的信息');
        $data['filterData'] = CleanerFilterChange::model()->getFilterData($cleaner,$type_id);
        //获取兑换总数
        if($data['filterData'])
        {
            foreach($data['filterData'] as $id=>$filter)
            {
                $data['filterData'][$id]['exchange_total'] = FilterExchange::model()->count(sprintf("cleaner_id = '%s' AND filter_id = %s ",$cleaner_id,$id));
            }
        }
        $data['activity_type'] = $type_id;
        $this->success($data);
    }

    /**
     * 兑换滤芯
     */
    public function actionExchange()
    {
        $cleaner_id = trim(Yii::app()->request->getParam('cleaner_id'));
        $filter_id = intval(Yii::app()->request->getParam('filter_id'));
        $name = trim(Yii::app()->request->getParam('name'));
        $mobile = trim(Yii::app()->request->getParam('mobile'));
        $address = trim(Yii::app()->request->getParam('address'));

        if(!$cleaner_id)
            $this->failed('参数错误');
        if(!$name || !$mobile || !$address)
            $this->failed('信息填写不完整');
        if(!Util::checkIsMobile($mobile))
            $this->failed('请输入正确的手机号码');

        list($type_id,$message) = CleanerFreeExchange::model()->checkCanJoinActivity($cleaner_id);
        if(!$type_id)
            throw new CHttpException(400, $message);

        $model = UserCleaner::model()->with('cleaner')->findByAttributes(array('cleaner_id' => $cleaner_id, 'user_id'=>$this->getUser()->id));
        if(!$model || $model->is_verify == UserCleaner::IS_VERIFY_NO)
            $this->failed('没有权限兑换滤芯');
        //判断净化器及权限
        $filterData = CleanerFilterChange::model()->getFilterData($model->cleaner,$type_id);
        if(!$filterData || !$filterData[$filter_id])
            $this->failed('没有找到滤芯相关信息');
        if($filterData[$filter_id]['can_exchange'] == false)
            $this->failed('条件不足，不能兑换滤芯');

        //判断是否已经兑换过
       /* $exchange = FilterExchange::model()->checkExchanged($cleaner_id,$filter_id);
        if($exchange)
            $this->failed('您已经兑换过滤芯了');*/
        $exchange = new FilterExchange();
        $exchange->attributes = array(
            'user_id' => $this->getUser()->id,
            'cleaner_id' => $cleaner_id,
            'filter_id'  => $filter_id,
            'create_time' => time(),
            'receiver_name' => $name,
            'receiver_mobile' => $mobile,
            'receiver_address' => $address,
        );
        if(!$exchange->save())
        {
            $this->failed($exchange->getErrors());
        }

        $this->success();
    }

    /**
     * 分享成功
     * @throws CHttpException
     */
    public function actionShareSuccess()
    {
        $cleaner_id = trim(Yii::app()->request->getParam('cleaner_id'));
        $filter_id = intval(Yii::app()->request->getParam('filter_id'));
        if(!$cleaner_id || !$filter_id || $filter_id >= 4)
            $this->failed('无效的参数');
        $model = UserCleaner::model()->with('cleaner')->findByAttributes(array('cleaner_id' => $cleaner_id, 'user_id'=>$this->getUser()->id));
        if(!$model || $model->is_verify == UserCleaner::IS_VERIFY_NO)
            $this->failed('无效的分享');
        $params = array(
            'cleaner_id' => $cleaner_id,
            'filter_id' => $filter_id,
            'date' => date('Ymd')
        );
        //判断今天是否分享了
        $share = WeixinShare::model()->findByAttributes($params);
        if(!$share)
        {
            $share = new WeixinShare();
            $share->attributes = $params;
            $share->add_time = time();
            $share->user_id = $this->getUser()->id;
            $share->save();
        }
        $this->success();
    }


}