<?php

/**
 * 
 * 滤芯剩余寿命推送日志
 * This is the model class for table "push_filter_log".
 *
 * The followings are the available columns in table 'push_filter_log':
 * @property string $id
 * @property string $cleaner_id
 * @property integer $filter_id
 * @property integer $cleaner_type
 * @property string $add_time
 * @property string $update_time
 * @property string $date
 */
class PushFilterLog extends CActiveRecord
{
	
	/**
	 * 推送标识 1 滤芯寿命为0 2滤芯寿命少于10%
	 * @var unknown
	 */
	const FLAG_OVER 	= 1;
	const FLAG_SURPLUS  = 2;
	
	
	/**
	 * @return string the associated database table name
	 */
	public function tableName()
	{
		return 'push_filter_log';
	}

	/**
	 * @return array validation rules for model attributes.
	 */
	public function rules()
	{
		// NOTE: you should only define rules for those attributes that
		// will receive user inputs.
		return array(
			array('cleaner_id, filter_id, cleaner_type, add_time, date', 'required'),
			// The following rule is used by search().
			// @todo Please remove those attributes that should not be searched.
			array('id, cleaner_id, filter_id, flag, cleaner_type, add_time, update_time, date', 'safe'),
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
		);
	}

	/**
	 * @return array customized attribute labels (name=>label)
	 */
	public function attributeLabels()
	{
		return array(
			'id' => 'ID',
			'cleaner_id' => 'Cleaner',
			'filter_id' => 'Filter',
			'cleaner_type' => 'Cleaner Type',
			'add_time' => 'Add Time',
			'update_time' => 'Update Time',
			'date' => 'Date',
		);
	}

	/**
	 * Returns the static model of the specified AR class.
	 * Please note that you should have this exact method in all your CActiveRecord descendants!
	 * @param string $className active record class name.
	 * @return PushFilterLog the static model class
	 */
	public static function model($className=__CLASS__)
	{
		return parent::model($className);
	}
	
	/**
	 * 检测是否已经推送过,update_time作为 标识符
	 * @param $cleaner_id 净化器id
	 * @param $filter_id 滤芯id(如果是带RFID的,则为滤芯类型id)
	 * @param $flag 1滤芯寿命为0 2滤芯寿命少于10%
	 * @return 推送过了返回true  未推送过返回false
	 */
	public function checkPush($cleaner_id, $filter_id, $flag=self::FLAG_OVER)
	{
		$sql = "SELECT * FROM " . $this->tableName() . " WHERE cleaner_id='{$cleaner_id}' and filter_id='{$filter_id}' and flag={$flag} ORDER BY id DESC LIMIT 1";
		$row = Yii::app()->db->createCommand($sql)->queryRow();
		//print_r($row);die;
		if (empty($row))
			return false;  // 未发送过
		else 
		{
			if (!empty($row['update_time']) && (time()-$row['update_time']>86400*2))
			{
				// 下一次未发送过
				return false;
			}
			else 
			{
				// 更新时间
				$sql = "UPDATE " . $this->tableName() . " SET update_time=" . time() . " WHERE id={$row['id']}";
				Yii::app()->db->createCommand($sql)->execute();
				return true;
			}
		}
	}
	
	/**
	 * 记录日志
	 * @param unknown $cleaner_id
	 * @param unknown $filter_id
	 * @param unknown $flag
	 */
	public function pushLog($cleaner_id, $filter_id, $cleaner_type, $flag=self::FLAG_OVER)
	{
		$model = new self();
		$model->attributes = array(
			'cleaner_id' => $cleaner_id,
			'filter_id'  => $filter_id,
			'cleaner_type' => $cleaner_type,
			'add_time' => time(),
			'date' => date('Y-m-d H:i:s', time()),
			'flag' => $flag
		);
		$model->save();
	}
}
