<?php

/**
 * This is the model class for table "user".
 *
 * The followings are the available columns in table 'user':
 * @property string $id
 * @property string $nickname
 * @property string $password
 * @property string $mobile
 * @property string $app_version
 * @property string $add_time
 * @property string $share_counts
 *
 * The followings are the available model relations:
 * @property CleanerStatus[] $cleanerStatuses
 * @property StatUserRank[] $statUserRanks
 * @property UserCleanerRel[] $userCleanerRels
 * @property UserFeedback[] $userFeedbacks
 * @property UserOperationLog[] $userOperationLogs
 */
class User extends CActiveRecord
{
    const VERIFY_NO = 0;  //未通过身份验证
    const VERIFY_YES = 1; //通过身份验证
	/**
	 * @return string the associated database table name
	 */
	public function tableName()
	{
		return 'user';
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
			array('nickname, password, app_version, add_time', 'required'),
			array('password', 'checkPassword', 'message' => '密码格式错误'),
			array('nickname', 'length', 'max'=>50),
            array('mobile', 'length', 'max'=>11),
            array('email', 'length', 'max'=>50),
            array('is_admin', 'length', 'max'=>1),
			array('email,is_verify', 'safe'),
			array('credit','numerical', 'integerOnly'=>true),
		);
	}
	
	/**
	 * 检测密码格式
	 * @param unknown $attribute
	 * @param unknown $param
	 */
	public function checkPassword($attribute, $param)
	{
		return true;
	}
	
	
	/**
	 * @return array relational rules.
	 */
	public function relations()
	{
		return array();
		// NOTE: you may need to adjust the relation name and the related
		// class name for the relations automatically generated below.
// 		return array(
// 			'cleanerStatuses' => array(self::HAS_MANY, 'CleanerStatus', 'operator_uid'),
// 			'statUserRanks' => array(self::HAS_MANY, 'StatUserRank', 'user_id'),
// 			'userCleanerRels' => array(self::HAS_MANY, 'UserCleanerRel', 'user_id'),
// 			'userFeedbacks' => array(self::HAS_MANY, 'UserFeedback', 'user_id'),
// 			'userOperationLogs' => array(self::HAS_MANY, 'UserOperationLog', 'user_id'),
// 		);
	}

	/**
	 * @return array customized attribute labels (name=>label)
	 */
	public function attributeLabels()
	{
		return array(
			'id' => 'ID',
			'nickname' => '昵称',
			'password' => '密码',
			'mobile' => '手机号',
			'app_version' => 'App版本',
			'add_time' => '注册时间',
			'share_counts' => 'Share Counts',
            'is_admin' => '超级账户',
			'credit'   => '积分'
		);
	}

	/**
	 * Returns the static model of the specified AR class.
	 * Please note that you should have this exact method in all your CActiveRecord descendants!
	 * @param string $className active record class name.
	 * @return User the static model class
	 */
	public static function model($className=__CLASS__)
	{
		return parent::model($className);
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
	
		$criteria->compare('nickname',$this->nickname,true);
		$criteria->compare('mobile',$this->mobile,true);
        $criteria->compare('email',$this->email,true);
	
		return new CActiveDataProvider($this, array(
				'criteria'=>$criteria,
				'sort' => array(
					'attributes' => array('id', 'mobile')
				)
		));
	}
	
	/**
	 * 注册时密码处理
	 * @see CActiveRecord::beforeSave()
	 */
	public function beforeSave()
	{
		if ($this->getIsNewRecord())
        {
            $this->password = $this->encrypting($this->password);
            //手机号码或邮箱唯一性判断
            if($this->is_verify == self::VERIFY_YES)
            {
                if($this->findByAttributes(array('mobile' => $this->mobile)))
                    throw new CHttpException(400, '手机号码已被注册');
            }else
            {
                if($this->findByAttributes(array('email' => $this->email)))
                    throw new CHttpException(400, '邮箱已被注册');
            }
        }

		return parent::beforeSave();
	}
	
	/**
	 * 验证码用户名密码对应的用户是否存在
	 * @return user 合法的用户对象
	 */
	public function userIdentity($mobile, $password)
	{
		if (Util::checkIsMobile($mobile))
			$model = $this->findByAttributes(array('mobile'=>$mobile));
		else 
			$model = $this->findByAttributes(array('email' => $mobile));
		if (empty($model))
			throw new CHttpException(404, '用户名不存在');
		if ($model->password != $this->encrypting($password))
			throw new CHttpException(400, '密码错误');
        if($model->is_verify == self::VERIFY_NO)
            throw new CHttpException(403, '您的账号还没有通过email验证，暂时不能使用');
		return $model;
	}
	
	/**
	 * 手机号和密码登录
	 * @param unknown $mobile
	 * @param unknown $password
	 * @param string $platform 平台系统
	 */
	public function login($mobile, $password)
	{
		$model = $this->userIdentity($mobile, $password);
		return $this->doLogin($model);
	}
	
	/**
	 * 登录
	 * @param User 用户对象 
	 * @return string 登录成功标识token
	 */
	public function doLogin($user)
	{
		Yii::app()->user->id = $user->id;
		Yii::app()->user->name = $user->nickname;
		// 判断是否已经登录
		$token = Token::model()->findByAttributes(array('user_id'=>$user->id));
		if (empty($token))
		{
			$token = new Token();
			$token->attributes = array(
				'user_id'  => $user->id,
				'token'    => Token::model()->generateToken($user->id),
				'platform' => Yii::app()->request->getParam('platform'),
				'add_time' => time()
			);
			$token->save();
		}
		return $token->token;
	}
	/**
	 * 密码加密
	 * @param unknown $string
	 */
	public function encrypting($password)
	{
		$key = 'AI#*R&*S@!Nee&&Be**rr';
		return md5($key . $password);
	}
	
	/**
	 * 验证密码的格式
	 * @param unknown $password
	 */
	public static function checkPwd($password)
	{
		$length = strlen($password);
		if ($length < 6 || $length > 12)
			return false;
		return true;
	}
	
	/**
	 * 统计用户总数
	 */
	public function calculateTotal()
	{
		$sql = "SELECT count(id) AS total FROM " . $this->tableName();
		return Yii::app()->db->createCommand($sql)->queryScalar();
	}

    public function afterSave()
    {
        parent::afterSave();
        //注册成功后发送邮件验证码
        if($this->getIsNewRecord() && $this->is_verify == self::VERIFY_NO)
        {
            if($this->email)
            {
                $this->sendEmail($this->getPrimaryKey(),$this->email);
            }
        }
    }

    private function sendEmail($id,$email)
    {
        /* Yii::app()->redis->rPush(EKeys::getWaitingSendEmailKey(),$this->getPrimaryKey().'#'.$this->email);
               return;*/
        Yii::import('ext.Xencrypt');
        $xen = new Xencrypt();
        $key = $xen->encode($id);
        $url = Yii::app()->createAbsoluteUrl('verify/index',array('key'=>$key,'sign'=>md5($email.'sangebaba_email_virify')));
        $content="亲爱的用户：<br>
感谢您注册三个爸爸客户端，您正在使用邮箱验证，请点击如下链接完成验证：<br>
".$url."<br>
(如无法点击，请复制链接到浏览器地址栏访问)<br><br>
三个爸爸空气净化器<br>
".date('Y年m月d日')."<br><br><br>
Dear Customer:<br>
You are almost done, click link below to complete your registration.<br>
".$url."<br>
(Copy URL and open with web browser in case of non-clickable link.)<br><br>
Three Papas Air Purifier<br>
".date('F d, Y');
        $success = Yii::app()->mail->send($email, '三个爸爸客户端--身份验证 Complete Registration With Three Papas', $content);
        //var_dump(Yii::app()->mail->getError());die;
    }
}
