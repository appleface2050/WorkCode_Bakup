<?php

/**
 * This is the model class for table "sms_log".
 *
 * The followings are the available columns in table 'sms_log':
 * @property string $id
 * @property string $mobile
 * @property string $content
 * @property integer $status
 * @property string $msg
 * @property string $create_time
 * @property integer $action
 */
class SmsLog extends CActiveRecord
{
	
	/**
	 * 手机发送 或者邮箱发送
	 * @var unknown
	 */
	const TYPE_MOBILE = 1;
	const TYPE_EMAIL  = 2;
	
	/**
	 * 短信发送 1成功 0失败
	 * @var unknown
	 */
	const SEND_SUCCESS = 1;
	const SEND_FAILED = 0;
	
	/**
	 * 短信用途 1注册
	 * @var unknown
	 */
	const ACTION_REGISTER = 1;
	
	/**
	 * 验证码有效期15分钟
	 * @var unknown
	 */
	protected $lifetime = 900;
	
	/**
	 * 输入完验证码后再注册最大验证码有效期
	 * @var unknown
	 */
	protected $maxtime = 1200;
	
	/**
	 * @return string the associated database table name
	 */
	public function tableName()
	{
		return 'sms_log';
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
			array('mobile, content, status, action, msg, add_time', 'required'),
		);
	}

	/**
	 * (non-PHPdoc)
	 * @see CActiveRecord::relations()
	 */
	public function relations()
	{
		return array();
	}
	
	
	/**
	 * @return array customized attribute labels (name=>label)
	 */
	public function attributeLabels()
	{
		return array(
			'id' => 'ID',
			'mobile' => 'Mobile',
			'content' => 'Content',
			'status' => 'Status',
			'msg' => 'Msg',
			'create_time' => 'Create Time',
			'action' => 'Action',
		);
	}
	
	public function scopes()
	{
		return array(
			'recently' => array(
				'order' => 'id DESC',
				'limit' => 1
			)
		);
	}
	
	
	/**
	 * Returns the static model of the specified AR class.
	 * Please note that you should have this exact method in all your CActiveRecord descendants!
	 * @param string $className active record class name.
	 * @return Sms the static model class
	 */
	public static function model($className=__CLASS__)
	{
		return parent::model($className);
	}
	
	/**
	 * 发送内容
	 * @param string $mobile
	 * @param unknown $content
	 */
	protected function send($mobile, $content, $action=self::ACTION_REGISTER)
	{
		$result = Yii::app()->sms->send($mobile, $content);
		$success = $result['status'] == 1 ? self::SEND_SUCCESS : self::SEND_FAILED;
		$model = new self();
		$model->attributes = array(
			'mobile' => $mobile,
			'content' => $content,
			'status' => $success,
			'msg' => $result['msg'],
			'action' => $action
		);
		$model->save();
		return $success ? true : false;
	}
	
	/**
	 * 
	 * 手机号发送验证码
	 * @param unknown $mobile
	 * @param return array(status=>1 , 'msg' => '');
	 */
	protected function sendByMobile($mobile, $content)
	{
		return Yii::app()->sms->send($mobile, $content);
	}
	
	/**
	 * 使用邮箱发送验证码
	 * @param unknown $mobile
	 */
	protected function sendByEmail($email, $contnet)
	{
		$success = Yii::app()->mail->send($email, '三个爸爸验证码', '三个爸爸验证码 ：' . $contnet);
		return $success ? array('status' => 1, 'msg' => '成功') : array('status' => 0, 'msg' => '失败');
	}
	
	/**
	 * 发送验证码
	 * @param unknown $mobile
	 * @param boole 成功true 失败false
	 */
	public function sendVerifyCode($mobile, $type=self::TYPE_MOBILE)
	{
		$code = Util::getVerifyCode();
		if ($type == self::TYPE_EMAIL)
			$result = $this->sendByEmail($mobile, $code);
		else
			$result = $this->sendByMobile($mobile, $code);
		$model = new self();
		$model->attributes = array(
			'mobile' => $mobile,
			'content' => $code,
			'status' => $result['status'],
			'msg' => $result['msg'],
			'action' => $type
		);
		$model->save();
		return $result['status'] == 1 ? true : false;
	}
	
	/**
	 * 检测验证码是否正确
	 * @param unknown $mobile
	 * @param unknown $code
	 */
	public function checkVerifyCode($mobile, $code)
	{
		$model = $this->recently()->findByAttributes(array('mobile'=>$mobile,'content'=>$code));
		if (empty($model) || $model->status != self::SEND_SUCCESS)
			throw new CHttpException(400, '验证码不存在');
		elseif (time() - $model->add_time > $this->lifetime)
			throw new CHttpException(400, '验证码已失效');
		else
			return true;
	}
	
	/**
	 * 检测最近的时间内是否给该手机号发送过验证码(注册前需要判断是否已发送手机号)
	 * @param unknown $mobile
	 */
	public function checkHasSend($mobile)
	{
		$model = $this->recently()->findByAttributes(array('mobile'=>$mobile));
		if (empty($model))
			throw new CHttpException(403, '非法请求');
		return true;
	}
}
