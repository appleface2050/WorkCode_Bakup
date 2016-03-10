<?php

/**
 * This is the model class for table "exchange_log".
 *
 * The followings are the available columns in table 'exchange_log':
 * @property string $id
 * @property string $user_id
 * @property string $goods_id
 * @property string $goods_name
 * @property integer $unlock_time
 * @property integer $cost_credit
 * @property string $receiver_name
 * @property string $receiver_mobile
 * @property string $receiver_address
 * @property integer $exchange_time
 */
class ExchangeLog extends CActiveRecord
{
	/**
	 * @return string the associated database table name
	 */
	public function tableName()
	{
		return 'exchange_log';
	}

	/**
	 * @return array validation rules for model attributes.
	 */
	public function rules()
	{
		// NOTE: you should only define rules for those attributes that
		// will receive user inputs.
		return array(
            array('exchange_time','default','value' => time()),
			array('user_id, goods_id, goods_name, unlock_time, cost_credit, exchange_time', 'required'),
			array('unlock_time, cost_credit, exchange_time', 'numerical', 'integerOnly'=>true),
			array('user_id, goods_id', 'length', 'max'=>10),
			array('goods_name', 'length', 'max'=>100),
			array('receiver_name, receiver_mobile, receiver_address', 'length', 'max'=>50),
			// The following rule is used by search().
			// @todo Please remove those attributes that should not be searched.
			array('id, user_id, goods_id, goods_name, unlock_time, cost_credit, receiver_name, receiver_mobile, receiver_address, exchange_time', 'safe', 'on'=>'search'),
		);
	}

	/**
	 * @return array relational rules.
	 */
	public function relations()
	{
		// NOTE: you may need to adjust the relation name and the related
		// class name for the relations automatically generated below.
		return array(
			'goods' => array( self::BELONGS_TO, 'Goods', 'goods_id' )
		);
	}

	/**
	 * @return array customized attribute labels (name=>label)
	 */
	public function attributeLabels()
	{
		return array(
			'id' => 'ID',
			'user_id' => 'User',
			'goods_id' => 'Goods',
			'goods_name' => 'Goods Name',
			'unlock_time' => 'Unlock Time',
			'cost_credit' => 'cost_credit',
			'receiver_name' => 'Receiver Name',
			'receiver_mobile' => 'Receiver Mobile',
			'receiver_address' => 'Receiver Address',
			'exchange_time' => 'Exchange Time',
		);
	}

	/**
	 * Retrieves a list of models based on the current search/filter conditions.
	 *
	 * Typical usecase:
	 * - Initialize the model fields with values from filter form.
	 * - Execute this method to get CActiveDataProvider instance which will filter
	 * models according to data in model fields.
	 * - Pass data provider to CGridView, CListView or any similar widget.
	 *
	 * @return CActiveDataProvider the data provider that can return the models
	 * based on the search/filter conditions.
	 */
	public function search()
	{
		// @todo Please modify the following code to remove attributes that should not be searched.

		$criteria=new CDbCriteria;

		$criteria->compare('id',$this->id,true);
		$criteria->compare('user_id',$this->user_id,true);
		$criteria->compare('goods_id',$this->goods_id,true);
		$criteria->compare('goods_name',$this->goods_name,true);
		$criteria->compare('unlock_time',$this->unlock_time);
		$criteria->compare('cost_credit',$this->cost_credit);
		$criteria->compare('receiver_name',$this->receiver_name,true);
		$criteria->compare('receiver_mobile',$this->receiver_mobile,true);
		$criteria->compare('receiver_address',$this->receiver_address,true);
		$criteria->compare('exchange_time',$this->exchange_time);

		return new CActiveDataProvider($this, array(
			'criteria'=>$criteria,
		));
	}

	/**
	 * Returns the static model of the specified AR class.
	 * Please note that you should have this exact method in all your CActiveRecord descendants!
	 * @param string $className active record class name.
	 * @return ExchangeLog the static model class
	 */
	public static function model($className=__CLASS__)
	{
		return parent::model($className);
	}

	public function scopes() {
		return array(
			'byiddesc' => array('order' => 'id DESC')
		);
	}
    /**
     * 判断是否可以退换某个商品
     * @param $user_id
     * @param $goods_id
     */
	public function checkExchange($user,$goods_id)
	{
        if(!$user || !$goods_id)
            return array(false,'请求参数错误');
        $user_id = $user->id;
        $goods = Goods::model()->findByPk($goods_id);
        if(!$goods || $goods->status == Goods::STATUS_OFFLINE)
            return array(false,'商品不存在或已下架，不能兑换');
        if($goods->quantity <= 0 )
            return array(false,'库存不足，无法兑换');
        if($user->credit < $goods->cost_credit)
            return array(false,'积分不足，无法兑换');
        $exchange = $this->byiddesc()->findByAttributes(array('user_id' => $user_id, 'goods_id' => $goods_id));
	    if(!$exchange)
            return array(true,$goods);
        if($exchange->unlock_time > time())
            return array(false,'解锁天数未到，还不能兑换此商品');
        return array(true,$goods);
    }

    /**
     * @param $user_id
     * @param $goods_id
     * @return array
     */
    public function exchange($user,$goods_id,$name,$mobile,$address)
    {
        $user_id = $user->id;
        list($status,$value) = $this->checkExchange($user,$goods_id);
        if(!$status)
            return array($status,$value);
        $goods = $value;
        $transaction = Yii::app()->db->beginTransaction();
        try{
            //扣除积分，记录兑换日志
            User::model()->updateCounters(array('credit'=> -($goods->cost_credit)),'id = '.$user_id);
            $exchangeLog = new self;
            $exchangeLog->attributes = array(
                'user_id' => $user_id,
				'goods_id' => $goods_id,
                'goods_name' => $goods->name,
                'unlock_time' => time() + $goods->unlock_days * 3600*24,
                'cost_credit' => $goods->cost_credit,
                'receiver_name' => $name,
                'receiver_mobile' => $mobile,
                'receiver_address' => $address,
            );
            $exchangeLog->save();
            //记录积分日志
            CreditLog::model()->saveLog($user_id,CreditLog::ACTION_EXCHANGE,$goods->cost_credit,'兑换商品:'.$goods->name);
            //更新商品兑换指数及减少库存
			Goods::model()->updateCounters(array('exchange_index'=>1,'quantity'=> -1),'id = '.$goods_id);
			$transaction->commit();
        }catch (Exception $e)
        {
            $transaction->rollback(); //操作失败, 数据回滚
            return array(false,'兑换失败：01');
        }
        return array(true,'兑换成功');
    }

	public static function statLeftUnlockDays($unlock_time)
	{
		return ceil( ($unlock_time - time())/3600/24 );
	}
}
