<?php
//ALTER TABLE `user` ADD COLUMN `credit` INT UNSIGNED DEFAULT 0 NOT NULL COMMENT '积分数' AFTER `is_verify`;
/**
 * 积分兑换相关
 *
 */
class ExchangeController extends RestController
{
    public function init()
    {
        header("Access-Control-Allow-Origin:*");
        parent::init();
    }
    //积分首页 获取商品信息
    public function actionIndex()
    {
        $data = array();
        $data['current_credit'] = $this->getUser()->credit;
        $criteria = new CDbCriteria();
        $criteria->order = 'Rank DESC';
        $criteria->addCondition('status = '.Goods::STATUS_ONLINE);      //根据条件查询
        $count = Goods::model()->count($criteria);
        $pager = new CPagination($count);
        $pager->pageSize=10;
        $pager->applyLimit($criteria);
        $result = Goods::model()->findAll($criteria);
        $data['goods'] = array();
        if($result)
        {
            foreach( $result as $key=>$val)
            {
                $data['goods'][$key] = array(
                   'id' => $val->id,
                   'name' => $val->name,
                   'cover' => AttachHelper::getFileUrl($val->cover),
                   'cost_credit' => $val->cost_credit,
                   'exchange_index' => $val->exchange_index,
                   'can_exchange' => true, //是否可以兑换 true:可以 ,false:不可以
                );
                //判断是否可以兑换该商品
                $exchange = ExchangeLog::model()->byiddesc()->findByAttributes(array('user_id' => Yii::app()->user->id, 'goods_id' => $val->id));
                if($exchange)
                {
                    if($exchange->unlock_time > time())
                    {
                        $data['goods'][$key]['can_exchange'] = false;
                        $data['goods'][$key]['left_unlock_days'] = ExchangeLog::statLeftUnlockDays($exchange->unlock_time);
                    }
                }
            }
        }
        $data['user'] = array(
            'mobile' => $this->getUser()->mobile
        );
        $this->success($data);
    }
    /**
     * 商品兑换接口
     */
    public function actionExchange()
    {
        $goods_id = intval(Yii::app()->request->getParam('goods_id',0));
        $name = trim(Yii::app()->request->getParam('name'));
        $mobile = trim(Yii::app()->request->getParam('mobile'));
        $address = trim(Yii::app()->request->getParam('address'));
        if(!$goods_id)
            $this->failed('参数错误');
        if(!$name || !$mobile || !$address)
            $this->failed('信息填写不完整');
        list($status,$message) = ExchangeLog::model()->exchange($this->getUser(),$goods_id,$name,$mobile,$address);
        if($status)
            $this->success();
        $this->failed($message);
    }
    /**
     * 获取积分记录
     */
    public function actionGetCreditLog()
    {
        $data = array();
        $criteria = new CDbCriteria();
        $criteria->order = 'id DESC';
        $criteria->addCondition('user_id = '.Yii::app()->user->id);      //根据条件查询
        $count = CreditLog::model()->count($criteria);
        $pager = new CPagination($count);
        $pager->pageSize=10;
        $pager->applyLimit($criteria);
        $result = CreditLog::model()->findAll($criteria);
        $data['credit_data'] = array();
        if($result)
        {
            foreach( $result as $val)
            {
                $data['credit_data'][] = array(
                    'type_id' => $val->type_id,    //加 或 减
                    'credit'     => $val->credit,  //积分
                    'time' => date('Y年m月d日',$val->create_time)
                );
            }
        }
        $data['current_credit'] = $this->getUser()->credit;
        $this->success($data);
    }
	/**
	 * 获取兑换记录列表
	 */
	public function actionGetExchangeLog()
	{
        $data = array();
        $criteria = new CDbCriteria();
        $criteria->order = 'id DESC';
        $criteria->addCondition('user_id = '.Yii::app()->user->id);      //根据条件查询
        $count = ExchangeLog::model()->count($criteria);
        $pager = new CPagination($count);
        $pager->pageSize=10;
        $pager->applyLimit($criteria);
        $result = ExchangeLog::model()->findAll($criteria);
        if($result)
        {
            foreach( $result as $val)
            {
                $data[] = array(
                        'goods_name' => $val->goods_name,
                        'cost_credit'     => $val->cost_credit,
                        'exchange_time' => date('Y.m.d',$val->exchange_time)
                );
            }
        }
        $this->success($data);
    }

    /**
     * 获取用户当前积分
     */
    public function actionGetCredit()
    {
        $this->success(array('credit' => $this->getUser()->credit));
    }
}