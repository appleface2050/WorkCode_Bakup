<?php

/**
 * This is the model class for table "cleaner_status".
 * 运行中的净化器
 * The followings are the available columns in table 'cleaner_status':
 * @property string $id
 * @property string $first_use_time
 * @property string $machine_id
 * @property string $filters_id
 * @property string $point_x
 * @property string $point_y
 * @property string $city
 * @property integer $level
 * @property integer $child_lock
 * @property integer $status
 * @property string $timeset  自动净化日期时间设置,序列号字符串,day start_time end_time 多个数组
 * @property integer $automatic
 * @property string $operator_uid
 * @property integer $pm25_index
 * @property string $update_time
 *
 * The followings are the available model relations:
 * @property UserCleanerRel[] $userCleanerRels
 * 
 * 
 */
class CleanerStatus extends CActiveRecord
{
	
	/**
	 * 机器类型 1大机器(不带RIFD) 2大机器(带RIFD) 3小机器  4 圆形机器(守护) 5伊娃
	 * @var unknown
	 */
	const TYPE_BIG_WITHOUT	 = 1; //高达老版
	const TYPE_BIG_WITH		 = 2;
	const TYPE_SMALL_WITHOUT = 3;
	const TYPE_CIRCULAR      = 4; //守护
	const TYPE_YIWA          = 5; //伊娃
	const TYPE_GAODA_NEW     = 6; //高达新版 07号段
	const TYPE_SHOUHU_SECOND = 7; //守护新版 08号段

	
	/**
	 * 净化器异常提示信息
	 * @var unknown
	 */
	const STATUS_EXCEPTION_TIP = '电源断开或WIFI未连接？';
	
	
	/**
	 * 净化器状态 1正常 0 异常(电源断开或WIFI未连接？)
	 * @var unknown
	 */
	const STATUS_NORMAL    = 1;
	const STATUS_EXCEPTION = 0;
	
	
	const VERSION_DEFAULT = '1.0.0';
	
	/**
	 * 瑞和固件升级初始版本
	 * @var unknown
	 */
	const UPGRADE_RUIHE_INIT = '2.0.0';
	
	/**
	 * 圆形机器固件升级版本
	 * @var unknown
	 */
	const UPGRADE_CIRCULE_INIT = '1.1.1';
	
	/**
	 * 1-4 档 0休眠 1静音
	 * @var unknown
	 */
	const LEVEL_OEN     = 2;
	const LEVEL_TWO     = 3;
	const LEVEL_THREE   = 4;
	const LEVEL_FOUR    = 5;
	const LEVEL_SLEEP   = 0;
	const LEVEL_SILENCE = 1;
	
	
	/**
	 * 净化器不同档位的净化量
	 */
	public static $cleanVolumnMap = array(
		self::LEVEL_OEN     => 200,
		self::LEVEL_TWO     => 300,
		self::LEVEL_THREE   => 400,
		self::LEVEL_FOUR    => 550,
		self::LEVEL_SLEEP   => 0,
		self::LEVEL_SILENCE => 150
	);

	/**
	 * 儿童锁,1开启0关闭
	 * @var unknown
	 */
	const CHILDLOCK_ON  = 1;
	const CHILDLOCK_OFF = 0;
	
	/**
	 * 自动净化 1开启0关闭
	 * @var unknown
	 */
	const AUTOMATIC_ON  = 1;
	const AUTOMATIC_OFF = 0;
	
	/**
	 * 净化器是否开始,1开启运行中0已关闭(即时休眠状态)
	 * @var unknown
	 */
	const SWITCH_OPEN  = 1;
	const SWITCH_CLOSE = 0;
	
	/**
	 * 静音时间设置是否开启,1开启0关闭
	 * @var unknown
	 */
	const SILENCE_ON  = 1;
	const SILENCE_OFF = 0;
	
	/**
	 * 默认的静音起止时间
	 * @var unknown
	 */
	const SILENCE_START_TIME = '21:00';
	const SILENCE_END_TIME   = '07:00';
	
	/**
	 * 操作类型
	 * @var unknown
	 */
	const OP_CODE_ONOFF 	= 1;
	const OP_CODE_CHILDLOCK = 2;
	const OP_CODE_AUTOMATIC = 3;
	const OP_CODE_LEVEL	    = 4;
	const OP_CODE_TIMESET   = 5;
	const OP_CODE_RESET	    = 6;  // 滤芯重置
	const OP_CODE_UPGRADE   = 7;  // 净化器在线升级
	const OP_CODE_MODIFY_FILTER = 16; //修改滤芯寿命
	const OP_CODE_LED_ONOFF = 17;  //LED灯开关控制
	
	/**
	 * 缓存的key
	 */
	const CHILDLOCK = 'childlock';
	
	
	// 是否已经绑定
	protected  $bind = false;
	
	/**
	 * @return string the associated database table name
	 */
	public function tableName()
	{
		return 'cleaner_status';
	}

	/**
	 * @return array validation rules for model attributes.
	 */
	public function rules()
	{
		// NOTE: you should only define rules for those attributes that
		// will receive user inputs.
		return array(
			array('first_use_time', 'default', 'value' => time()),
			array('id, first_use_time, qrcode, filter_surplus_life', 'required'),
			array('led_status,air_level,type', 'numerical', 'integerOnly'=>true),
			//array('qrcode', 'unique', 'message' => '该序列号已经使用过了'),
			// The following rule is used by search().
			// @todo Please remove those attributes that should not be searched.
			array('id, first_use_time, qrcode, point_x, point_y, city, level, switch, child_lock, status, timeset, automatic, operator_uid, aqi, update_time', 'safe'),
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
			'userCleanerRels' => array(self::HAS_MANY, 'UserCleanerRel', 'cleaner_id'),
			'operator' => array(self::BELONGS_TO, 'User', 'operator_uid')
		);
	}

	/**
	 * @return array customized attribute labels (name=>label)
	 */
	public function attributeLabels()
	{
		return array(
			'id' => 'ID(MAC地址)',
			'qrcode' => '序列号',
			'first_use_time' => '首次使用时间',
			'filter_surplus_life' => '滤芯寿命单元数值',
			'point_x' => '纬度',
			'point_y' => '经度',
			'aqi' => 'Pm25',
			'city' => '城市',
			'level' => '档位',
			'childlock' => '童锁',
			'status' => '状态',
			'time_set' => 'Time Set',
			'automatic' => '自动',
			'operator_uid' => '操作人',
			'switch' => '开关',
			'update_time' => '更新时间',
			'temperature' => '温度',
			'humidity' => '湿度',
			'led_status' => 'led状态',
			'air_level' => '空气质量等级',
			'version' => '固件版本'
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
		$criteria->compare('qrcode',$this->qrcode,true);

		return new CActiveDataProvider($this, array(
			'criteria'=>$criteria,
			'sort'=>array(
					//'defaultOrder' => 'id desc',
					'attributes' => array('id','qrcode')
			)
		));
	}

	/**
	 * Returns the static model of the specified AR class.
	 * Please note that you should have this exact method in all your CActiveRecord descendants!
	 * @param string $className active record class name.
	 * @return CleanerStatus the static model class
	 */
	public static function model($className=__CLASS__)
	{
		return parent::model($className);
	}
	
	/**
	 * (non-PHPdoc)
	 * @see CActiveRecord::beforeSave()
	 */
	public function beforeSave()
	{
		if (!Yii::app()->user->is_admin)
		{
	 		// 非测试号码, 判断序列号是否合法
	 		//Yii::log($this->qrcode);
			$cleaner = Cleaner::model()->findByAttributes(array('qrcode' => $this->qrcode));
			//Yii::log(var_dump($cleaner));
			
			if (empty($cleaner))
				throw new CHttpException(403, '序列号不存在');
	
			
			// 判断是否已经绑定过了
			if (!empty($cleaner->cleaner_id) && ($this->id != $cleaner->cleaner_id))
			{
				throw new CHttpException(400, '该序列号已经绑定过了！');
			}
			
			if (!$this->getIsNewRecord() && !empty($cleaner->cleaner_id))
			{
				// 已经绑定过的, 判断序列号和id是否完全匹配
				if (($this->id != $cleaner->cleaner_id) || ($this->qrcode != $cleaner->qrcode))
					throw new CHttpException(400, '请输入已绑定的序列号');
			}
			
			if (empty($cleaner->qrcode))
				$this->bind = true;
		}
		return parent::beforeSave();
	}
	
	
	/**
	 * (non-PHPdoc)
	 * @see CActiveRecord::afterSave()
	 */
	public function afterSave()
	{
		if (!Yii::app()->user->is_admin)
		{
			if ($this->getIsNewRecord() || (!$this->bind))
			{
				// 新添加的,首次绑定的 或者 已经添加(净化器联网过, 但未通过APP添加成功)
				$sql = "UPDATE ".Cleaner::model()->tableName()." SET cleaner_id='" . $this->id . "' WHERE qrcode='" . $this->qrcode . "'";
				Yii::app()->db->createCommand($sql)->execute();
			}
		}
		return parent::afterSave();
	}
	
	/**
	 * 检测level是否合法
	 * @param unknown $level
	 */
	public static function checkLevel($level)
	{
		if ($level == self::LEVEL_OEN || $level == self::LEVEL_TWO || $level == self::LEVEL_THREE || $level == self::LEVEL_FOUR || $level == self::LEVEL_SILENCE || $level == self::LEVEL_SLEEP)
			return true;
		else 
			return false;
	}
	
	
	/**
	 * 净化器的详细信息,包括pm25值,status为1净化器正常运行 为0净化器关闭状态
	 * @param unknown $id
	 */
	public function detail($id)
	{
		$model = $this->with('operator')->findByPk($id);
		if (empty($model))
			throw new CHttpException(404, '净化器不存在');
		$operator = $model->operator;
		$rel = UserCleaner::model()->findByAttributes(array('cleaner_id' => $id, 'user_id' => Yii::app()->user->id));
		$data = array(
			'id' => $model->id,
			'type' => intval($model->type),
			'status' => intval($model->status),
			'level' => intval($model->level),
			'is_open' => intval($model->switch),
			'childlock' => intval($model->childlock),
			'automatic' => intval($model->automatic),
			'aqi' => intval($model->aqi),  // 空气质量指数 大机器是pm25 小机器是空气质量优劣标识(0-100)
			'pm_outer_index' => Yii::app()->location->getPm($model->city),
			'tip' => self::getFilterTip(json_decode($model->filter_surplus_life, true)),
			'last_operator' => (!empty($operator) && $model->operator_uid != Yii::app()->user->id) ? $operator->nickname : '',
			'name' => !empty($rel) ? $rel->name : '',
			'voc' => intval($model->voc),
			'timeSet' => empty($model->timeset) ? array() : json_decode($model->timeset,true),
			'in_timeset' => $this->checkInSilence($model)
		);
		$data['air_quality'] = CleanerStatus::getAirQuality($model->air_level);
		$data['led_status'] = intval($model->led_status);
		$data['temperature'] = intval($model->temperature);
		$data['humidity'] = intval($model->humidity);
		return $data;
	}
	
	/**
	 * 获取格式化后的净化时间设置,主要用来传递给净化器端参数
	 * @param string $timeset 多个时间设置 每一个包含的key为 day start_tim end_time
	 * @param obj $cleaner 净化器对象
	 */
	public function getFormatTimeset($cleaner)
	{
		if (empty($cleaner))
			return false;
		$timeset = json_decode($cleaner->timeset,true);
		$silence = intval($cleaner->silence);  // 是否开启静音时间设置
		$silence_start = str_replace(':', '', $cleaner->silence_start);
		$silence_end = str_replace(':', '', $cleaner->silence_end);
		if ($silence_start > $silence_end)
		{
			// 跨天
			$silence_start_1 = '0000';
			$silence_end_1 = $silence_end;
			$silence_start_2 = $silence_start;
			$silence_end_2 = '2359';
		} else 
		{
			$silence_start_1 = $silence_start;
			$silence_end_1 = $silence_end;
			$silence_start_2 = '0000';
			$silence_end_2 = '0000';
		}
		
		$data = array_fill(1, 7, array());
		$time = array();
		if (!empty($timeset))
		{
			foreach ($timeset as $value)
			{
				if (intval($value['open'] == 1) && !empty($value['day']))
				{
					// 开启的自动净化时间
					$days = explode(',', $value['day']); // 哪几天,可能多个 也可能一个
					foreach ($days as $day)
					{
						$start_time = str_replace(':', '', $value['start_time']);
						$end_time = str_replace(':', '', $value['end_time']);
						
						if ($start_time > $end_time)
						{
							// 夸天
							//array_unshift($data[$day], array('start_time' => $end_time,'end_time'   => '2359'));
							$data[$day][] = array(
								'start_time' => $start_time,
								'end_time'   => '2359'
							);
							if ($day == 7)
								$day = 1;
							else 
								$day += 1;
							$data[$day][] = array(
								'start_time' => '0000',
								'end_time'   => $end_time
							);
						} else {
							// 没跨天
							$data[$day][] = array(
								'start_time' => $start_time,
								'end_time' => $end_time
							);
						}
					}
				}
			}
			//print_r($data);die;
			
			foreach ($data as $day => &$set)
			{
				// 每天的很多歌自动净化时段,但是不包括跨天的, 处理多个可能较差的时间段
				$total = count($set);
				if ($total > 1)
				{
					//$tmp[] = $set[0];
					// 多余一个时间段, 先按起止时间从小到大排序
					foreach ($set as $key => $value)
					{
						$start[$key] = $value['start_time'];
					}
					// 按开始时间从小到大排序
					array_multisort($start, SORT_ASC , $set);
					
					$current = array();
					
					for($i=0; $i<$total; $i++)
					{
						for($j=$i+1; $j<$total; $j++)
						{
							// 处理交叉的
							if ($set[$j]['start_time'] < $set[$i]['end_time'])
							{
								$set[$i]['end_time'] = $set[$j]['end_time'];
								unset($set[$j]);
							}
						}
					}
				}
			}
		}
		
		//
		foreach ($data as $day => $val)
		{
			// 遍历7天
			if ($day == 7)
			{
				$day = 0;
			}
				
			$tmp = '';
			if (!empty($val))
			{
				$first = array_shift($val);
				$tmp = $first['start_time'] . $first['end_time'];
				if (count($val)>0)
				{
					$second = array_shift($val);
					$tmp .= '^' . $second['start_time'] . $second['end_time'];
				}
				else
					$tmp .= '^00000000';
				
				$tmp .= '^00000000^00000000';
				
// 				if ($silence == self::SILENCE_ON)
// 					$tmp .= '^' . $silence_start_1. $silence_end_1 . '^' . $silence_start_2 . $silence_end_2;
// 				else
// 					$tmp .= '^00000000^00000000';
			} else {
				
				$tmp = '00000000^00000000^00000000^00000000';
// 				if ($silence == self::SILENCE_ON)
// 				{
// 					// 有静音时间设置
// 					$tmp = '00000000^00000000^' . $silence_start_1 . $silence_end_1 . '^' . $silence_start_2 . $silence_end_2;
// 				} else
// 				{
// 					$tmp = '00000000^00000000^00000000^00000000';
// 				}
			}
			$time[] = $day . '|' . $tmp;
		}
		return $time;
	}

	/**
	 * @param int $type  净化器类型
	 * @param int $id    滤芯id
	 * @param string $firmware_version  固件版本号
	 */
	public static function getDefaultLife($type=self::TYPE_BIG_WITHOUT, $id=null,$firmware_version=null)
	{
		$map = array(
			// 带有RIFD的 1 2 3表示分类
			self::TYPE_BIG_WITHOUT => array(
				1 => 300000,
				2 => 300000,
				3 => 300000
			),
			self::TYPE_BIG_WITH => array(
				1 => 300000,
				2 => 300000,
				3 => 300000
			),
			self::TYPE_SMALL_WITHOUT => array(
				1 => 300000,
				2 => 300000,
				3 => 300000
			),
			self::TYPE_CIRCULAR => array(
				1 => 300000
			),
			self::TYPE_YIWA => array(
				1 => 700000
			),
			self::TYPE_GAODA_NEW => array(
				1 => 700000,
				2 => 700000,
				3 => 700000
			),
            self::TYPE_SHOUHU_SECOND => array(
                1 => 700000
            )
		);

		//高达滤芯寿命 根据固件版本号 特殊处理
		if($type == self::TYPE_BIG_WITHOUT)
		{
			if($firmware_version)
			{
				if($firmware_version > '2.1.0')
				{
					$map[$type] =  array(
						1 => 700000,
						2 => 700000,
						3 => 700000
					);
				}elseif(intval($firmware_version) < 2) //滤芯寿命异常版本判断 都是小于2.0.0的版本有问题
				{
					$map[$type] =  array(
						1 => 28500,
						2 => 72000,
						3 => 9500
					);
				}
			}
		}
		if (!empty($id))
			return isset($map[$type][$id]) ? $map[$type][$id] : 0;
		else
			return $map[$type];
	}
	
	/**
	 * 获取滤芯默认的名字
	 * @param unknown $type
	 * @param unknown $id
	 */
	public static function getDefaultName($type=self::TYPE_BIG_WITHOUT, $id=null)
	{
		$map = array(
			self::TYPE_BIG_WITHOUT => array(
				1 => 'Fine Filtrete HEPA',
				2 => 'Advanced Pollution Control',
				3 => 'Ultra Filtrete HEPA'
			),
			self::TYPE_BIG_WITH => array(
				1 => 'Ultra Filtrete HEPA',
				2 => 'Advanced Pollution Control',
				3 => 'Fine Filtrete HEPA'
			),
			self::TYPE_SMALL_WITHOUT => array(
				1 => 'Pre-HEPA',
				2 => 'Super-HEPA',
				3 => '甲醛滤芯'
			),
			self::TYPE_CIRCULAR => array(
				1 => '桶形滤芯'
			),
			self::TYPE_YIWA => array(
				1 => 'H11级桶形HEPA滤芯'
			),
			self::TYPE_GAODA_NEW => array(
				1 => 'Fine Filtrete HEPA',
				2 => 'Advanced Pollution Control',
				3 => 'Ultra Filtrete HEPA'
			),
            self::TYPE_SHOUHU_SECOND => array(
                1 => '桶形滤芯'
            ),
		);
		if (!empty($id) && isset($map[$type][$id]))
			return $map[$type][$id];
		else
			return $map[$type];
	}
	
	/**
	 * 滤芯寿命快过期的提示
	 * @param unknown $data  净化器剩余寿命数组
	 */
	public function getFilterTip($data, $type = self::TYPE_BIG_WITH)
	{
		$tip = '';
		if (!empty($data))
		{
			foreach ($data as $id => $life)
			{
				if (intval($life)<5)
				{
					$tip = '请更换滤芯' . self::getDefaultName($type, $id);
					break;
				}
			}
		}
		return $tip;
	}
	
	/**
	 * 检测是否可以给机器发生指令
	 * @todo 根据指令的不同进行不同的判断
	 * @param unknown $model
	 */
	public function checkControl($model)
	{
		if ($model->status == self::STATUS_EXCEPTION || $model->switch == self::SWITCH_CLOSE)
			return false;
		return true;
	}
	
	/**
	 * 检测是否需要
	 * 1 提示
	 * 2 不提示
	 */
	public function checkInSilence($model)
	{
		
		if (empty($model->timeset))
		{
			// 为空, 不提示
			return 0;
		}
		
		if ($model->automatic == self::AUTOMATIC_OFF)
			return 0;  // 手动 不提示
		
		
		// 获取当前的日期时间
		$tmp = getdate();
		$today = $tmp['wday'];  // 0-6   变为  1-7
		if ($today == 0)
			$today = 1;
		
		
		$hour = $tmp['hours'];
		$minutes = $tmp['minutes'];
	//	$time =  . ':' . minutes;   // 当前的小时分钟
		
		
		$set = array();
		$timeset = json_decode($model->timeset,true);
		//print_r($timeset);die;
		foreach ($timeset as $value)
		{
			if (intval($value['open'] == 1))
			{
				// 开启的
				$days = explode(',', $value['day']); // 哪几天,可能多个 也可能一个
				//print_r($days);die;
				if (in_array($today, $days))
				{
					$set[] = array('start_time' => $value['start_time'],  'end_time' => $value['end_time']);
				}
			}
		}
		
		$in_timeset = 0;
		
		if (!empty($set))
		{
			foreach ($set as $value)
			{
				$start = explode(':', $value['start_time']);
				$end = explode(':', $value['end_time']);
				if ($hour >= $start[0])
				{
					if ($hour < $end[0])
					{
						$in_timeset = 1;
						break;
					}
					
					if ($end[0] < $hour)
					{
						// 跨天
						$in_timeset = 1;
						break;
					}
					
					if ($hour == $start[0])
					{
						// 相同的小时
						if ($minutes > $start[1])
						{
							$in_timeset = 1;
							break;
						}
					}
				}
				
			}
		}
		return $in_timeset;
	}
	
	/**
	 * 根据净化器类型 获取客户端id
	 * @param unknown $client_id
	 * @return string
	 */
	private function map($client_id)
	{
		$map = self::getMap();
		return isset($map[$client_id]) ? $map[$client_id] : '';
	}
	
	
	/**
	 * 检测是否需要升级
	 * @param unknown $cleaner
	 * @return 升级 返回最新版本  不升级 false
	 */
	public function checkUpgrade($cleaner) 
	{
		$client_id = $this->map($cleaner->type);
		if (empty($client_id))
			return false;
		
		
		if ($cleaner->type == self::TYPE_BIG_WITHOUT && ($cleaner->version < self::UPGRADE_RUIHE_INIT))
		{
			// 已经上市的净化器 没有在线升级的功能
			return false;
			
		}
		
// 		if (empty($cleaner->version))
// 			$cleaner->version = self::VERSION_DEFAULT;
		
		
		$latest = UpgradeProgram::model()->getLatestVersion($client_id);
		
		
		//$upgradeVersion = $cleaner->type == self::TYPE_CIRCULAR ? self::UPGRADE_CIRCULE_INIT : self::UPGRADE_RUIHE_INIT;

        //高达 3.0.2以下不能升级
        if($cleaner->type == self::TYPE_GAODA_NEW)
        {
            if($cleaner->version < '3.0.2')
                return false;
        }

		if ($latest > ($cleaner->version))
			return $latest;
		
		return false;
		
// 		// 初始默认版本
// 		if (empty($cleaner->version))
// 			$cleaner->version = self::VERSION_DEFAULT;
		
// 		// 不包含固件升级功能
// 		if ($cleaner->version < self::VERSION_UPGRADE_INIT)
// 			return false;
		
// 		$latest = UpgradeProgram::model()->getLatestVersion($client_id);
		
// 		if (empty($latest))
// 			return false;
		
// 		return $latest > ($cleaner->version) ? $latest : false;
		
	}

    /**
     * 统计净化器滤芯寿命（百分比）
     * @param $surplus_life
     * @param $type
	 * @param $firmware_version 固件版本
     * @return array
     */
    public static function countSurplusLife($surplus_life,$type,$firmware_version=null)
    {
        //原始基数
        $localFullLife = 300000;
        if(!$surplus_life)
            return array();
        $surplus_life = json_decode($surplus_life, true);
        $surplus = array();
        $i=1;
        $is_exception_version = false;
        if(in_array($type,array(self::TYPE_BIG_WITHOUT,self::TYPE_CIRCULAR)) && $firmware_version && intval($firmware_version) < 2)
        {
            $is_exception_version = true;
        }
        foreach ($surplus_life as $id => $life)
        {
            $defaultLife = intval(self::getDefaultLife($type, $i,$firmware_version));
            //计算规则判断
            if($is_exception_version)
            {
                $left = intval($defaultLife - $localFullLife + $life);
                $per = number_format($left/$defaultLife, 2) * 100;
            }else
            {
                $per = number_format(intval($life)/$defaultLife, 2) * 100;
            }
            if($per > 100)
            {
                $per = 100;
            }elseif($per < 1)
            {
                $per = 1;
            }
            $surplus[] = array('id' => $id, 'name' => self::getDefaultName($type, $i), 'life' => $life, 'surplus_life' => $per . '%', 'life_value' => $per);
            $i++;
        }
        return $surplus;
    }

	/**
	 * 根据等级获取空气质量
	 * @param $air_level
	 */
	public static function getAirQuality($air_level = 0)
	{
		if(!$air_level)
			return '';
		static $arr = array(
			1 => '优',
			2 => '良',
			3 => '差',
		);
		return isset($arr[$air_level]) ? $arr[$air_level] : '';
	}

	/**
	 * 获取净化器类型
	 * @param $short_qrcode 管理员手动输入的简短二维码
	 * @return int
	 */
	public static function getCleanerType($short_qrcode)
	{
		$prefix = substr($short_qrcode,0,2);
		/*if(!in_array($prefix,array('01','03','06')))
			return 0;*/
		$type = 0;
		switch($prefix)
		{
			case '01':
				$type = self::TYPE_BIG_WITHOUT;
				break;
			case '03':
				$type = self::TYPE_CIRCULAR;
				break;
			case '06':
				$type = self::TYPE_YIWA;
				break;
			case '07':
				$type = self::TYPE_GAODA_NEW;
				break;
            case '08':
                $type = self::TYPE_SHOUHU_SECOND;
                break;
		}
		return $type;
	}

    /**
     * 类型与客户端id映射
     * @return array
     */
    public static function getMap()
    {
        $map = array(
            self::TYPE_BIG_WITHOUT => '100',
            self::TYPE_CIRCULAR => '300',
            self::TYPE_YIWA => '201',
            self::TYPE_GAODA_NEW => '110',
            self::TYPE_SHOUHU_SECOND => '310'
        );
        return $map;
    }
}
