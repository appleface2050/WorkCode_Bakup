<?php

/**
 * This is the model class for table "user_cleaner_rel".
 *
 * The followings are the available columns in table 'user_cleaner_rel':
 * @property integer $id
 * @property string $user_id
 * @property string $cleaner_id
 * @property string $name
 * @property string $wifi_name
 * @property string $wifi_pwd
 * @property string $add_time
 * @property integer $status
 *
 * The followings are the available model relations:
 * @property CleanerStatus $cleaner
 */
class UserCleaner extends CActiveRecord
{
	
	/**
	 * 状态 0已删除 1正常
	 * @var unknown
	 */
	const STATUS_DELETEED = 0;
	const STATUS_NORMAL   = 1;
	
	/**
	 * 是否是主账号
	 * Enter description here ...
	 * @var unknown_type
	 */
	const IS_CHARGER_YES = 1;
	const IS_CHARGER_NO  = 0;
	
	/**
	 * 是否审核通过
	 * Enter description here ...
	 * @var unknown_type
	 */
	const IS_VERIFY_YES = 1;
	const IS_VERIFY_NO  = 0;
	
	/**
	 * @return string the associated database table name
	 */
	public function tableName()
	{
		return 'user_cleaner_rel';
	}

	/**
	 * @return array validation rules for model attributes.
	 */
	public function rules()
	{
		// NOTE: you should only define rules for those attributes that
		// will receive user inputs.
		return array(
			array('add_time', 'default', 'value' => time()),
			array('user_id, cleaner_id, name, add_time', 'required', 'message' => '净化器名称不可为空'),
			array('name', 'length', 'max'=>20,'message'=>'名字不能超过20个字'),
			array('point_x, point_y, wifi_name, wifi_pwd, city, add_time, is_charger, is_verify', 'safe'),
		
			// The following rule is used by search().
			// @todo Please remove those attributes that should not be searched.
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
			'cleaner' => array(self::BELONGS_TO, 'CleanerStatus', 'cleaner_id'),
			'user'    => array(self::BELONGS_TO, 'User', 'user_id')
		);
	}
	
	public function scopes()
	{
		$alias = $this->getTableAlias(false, false);
		
		return array(
			'early' => array(
				'order' => $alias . '.add_time DESC'
			),
			'bindAsc' => array(
				'order' => $alias . '.add_time ASC'
			)
		);
		
	}
	
	/**
	 * @return array customized attribute labels (name=>label)
	 */
	public function attributeLabels()
	{
		return array(
			'id' => 'ID',
			'user_id' => '关联用户',
			'cleaner_id' => '净化器ID',
			'name' => '净化器名称',
			'wifi_name' => 'Wifi Name',
			'wifi_pwd' => 'Wifi Pwd',
			'add_time' => '添加时间',
			'is_charger' => '是否主账号',
			'is_verify'  => '是否审核通过'
		);
	}

	/**
	 * Returns the static model of the specified AR class.
	 * Please note that you should have this exact method in all your CActiveRecord descendants!
	 * @param string $className active record class name.
	 * @return UserCleaner the static model class
	 */
	public static function model($className=__CLASS__)
	{
		return parent::model($className);
	}
	
	/**
	 * wifi_pwd 处理
	 * @see CActiveRecord::beforeSave()
	 */
	public function beforeSave()
	{
		if ($this->getIsNewRecord())
		{
			// 判断库里 是否有该净化器
// 			$cleaner = Cleaner::model()->findByPk($this->cleaner_id);
// 			if (empty($cleaner))
// 				throw new CHttpException(404, '净化器不存在,您可能买到假的了！');
			$this->wifi_pwd = !empty($this->wifi_pwd) ? User::model()->encrypting($this->wifi_pwd) : '';
		}
		return parent::beforeSave();
	}
	
	
	/**
	 * 获取用户的静音时间设置(1个用户有多个净化器,每个净化器的静音时间时一样的)
	 * @param unknown $user_id
	 */
	public function getSilenceSet($user_id)
	{
		$data = array('silence' => CleanerStatus::SILENCE_ON, 'start_time' => CleanerStatus::SILENCE_START_TIME, 'end_time' => CleanerStatus::SILENCE_END_TIME);
		$model = self::model()->with('cleaner')->findByAttributes(array('user_id'=>$user_id));
		if (!empty($model)) 
		{
			$cleaner= $model->cleaner;
			if (!empty($cleaner))
			{
				$data['silence'] = intval($cleaner->silence);
				if (!empty($cleaner->silence_start))
					$data['start_time'] = $cleaner->silence_start;
				if (!empty($cleaner->silence_end))
					$data['end_time'] = $cleaner->silence_end;
			}
		}
		return $data;
	}

    /**
     * 后台查询
     * @return CActiveDataProvider
     */
    public function search()
    {
        // @todo Please modify the following code to remove attributes that should not be searched.

        $criteria=new CDbCriteria;
        $criteria->compare('cleaner_id',$this->cleaner_id,true);
        return new CActiveDataProvider($this, array(
            'criteria'=>$criteria,
        ));
    }
    
    /**
     * 检测净化器是否有主账户
     * Enter description here ...
     * @param unknown_type $user_id
     * @param unknown_type $cleaner_id
     */
    public function checkHasCharger($cleaner_id)
    {
    	$model = $this->findByAttributes(array('cleaner_id' => $cleaner_id, 'is_charger' => self::IS_CHARGER_YES, 'is_verify' => self::IS_VERIFY_YES));
		return !empty($model) ? true : false;
    	
    }
    
    /**
     * 检查是否是主账户
     * Enter description here ...
     */
    public function checkIsCharger($user_id, $cleaner_id)
    {
    	$model = $this->findByAttributes(array('cleaner_id' => $cleaner_id, 'user_id' => $user_id));
		return (!empty($model) && $model->is_charger == self::IS_CHARGER_YES) ? true : false;
    }
    
    /**
     * 推送申请
     * @param $user user对象  或者 属性数组
     * @param $cleaner_id 净化器id
     */
    public function apply($user, $cleaner_id)
    {
    	/*$chargers = $this->findAllByAttributes(array('cleaner_id' => $cleaner_id, 'is_charger' => self::IS_CHARGER_YES, 'is_verify' => self::IS_VERIFY_YES));
    	if (!empty($chargers))
    	{
    		foreach ($chargers as $value)
    		{*/
    			//$applyLog = UserApplyLog::model()->findByAttributes(array('apply_uid' => $user['id'], 'cleaner_id' => $cleaner_id, 'apply_tid' => $value->user_id));
				$applyLog = UserApplyLog::model()->findByAttributes(array('apply_uid' => $user['id'], 'cleaner_id' => $cleaner_id));
				if (empty($applyLog))
    			{
    				$applyLog = new UserApplyLog();
    				$applyLog->attributes = array(
    					'apply_uid'  => $user['id'],
    					'apply_tid'  => 0,
    					'cleaner_id' => $cleaner_id,
    				);
    			}
    			$applyLog->add_time = time();
    			$applyLog->status = UserApplyLog::APPLY_STATUS_APPLY;
    			$applyLog->save();
    			
    			// 推送通知
    			/*$data = array(
					'title'   => '三个爸爸',
					'content' => $user['nickname'] . '申请添加净化器' . $value['name'],
					'info_id'  => 4
				);
				Yii::app()->push->push($value->user_id, $data);	*/
    	/*	}
    	}*/
    }

	/**
	 * 判断是否绑定某个净化器
	 * Enter description here ...
	 */
	public function checkHasBind($user_id, $cleaner_id)
	{
		$model = $this->findByAttributes(array('cleaner_id' => $cleaner_id, 'user_id' => $user_id));
		return (!empty($model) && $model->is_verify == self::IS_VERIFY_YES) ? $model : false;
	}
}
