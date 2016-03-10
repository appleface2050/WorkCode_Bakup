<?php

/**
 * 登录
 * @author zhoujianjun
 *
 */
class LoginController extends RestController
{
	
	/**
	 * (non-PHPdoc)
	 * @see RestController::beforeAction()
	 */
	public function beforeAction($action)
	{
		return true;
	}
	
	/**
	 * 发送验证码
	 * @param string $mobile
	 * @param string $sign
	 * 
	 * @add 增加邮箱注册  2015-03-25
	 */
	public function actionSendVerifyCode()
	{	
		$this->addParam();
		$this->checkSign(array('mobile','client_id'));
		$mobile = Yii::app()->request->getParam('mobile');
		$isMobile = $this->checkType($mobile);
		// 最好去掉
		$action = Yii::app()->request->getParam('action', '');
		if ($action == 'register') 
		{
			// 注册, 检测是否已经注册过
			if ($isMobile)
				$user = User::model()->findByAttributes(array('mobile' => $mobile));
			else
				$user = User::model()->findByAttributes(array('email' => $mobile));
			if (!empty($user))
				$this->failed('账号已经注册');
		}
		$type = $isMobile ? SmsLog::TYPE_MOBILE : SmsLog::TYPE_EMAIL;
		if (SmsLog::model()->sendVerifyCode($mobile, $type))
			$this->success();
		else 
			$this->failed('短信发送失败');
	}
	
	/**
	 * 检测验证码是否正确
	 * @param string $mobile
	 * @param int $login 1 验证码正确后登录 0 不登录
	 * @param string $code 验证码
	 */
	public function actionCheckVerifyCode()
	{
		$this->addParam();
		$mobile = Yii::app()->request->getParam('mobile');
		$code = Yii::app()->request->getParam('code');
		if (!Util::checkVerifyCode($code))
			throw new CHttpException(400, '验证码错误');
		$this->checkSign(array('mobile','code','client_id'));
		if (SmsLog::model()->checkVerifyCode($mobile, $code))
		{
			Yii::app()->redis->setex($mobile, 200, md5($mobile));
			$this->success();
		}
		else 
			$this->failed('验证码错误');
	}
	
	/**
	 * 检测手机号是否已经注册了 1 已注册 0 未注册
	 * @param string mobile 手机号
	 */
	public function actionCheckMobileExist()
	{
		$mobile = Yii::app()->request->getParam('mobile');
		$this->addParam();
		$this->checkSign(array('mobile','client_id'));
		$isMobile = $this->checkType($mobile);
		if ($isMobile)
			$member = User::model()->findByAttributes(array('mobile' => $mobile));
		else
			$member = User::model()->findByAttributes(array('email' => $mobile));
		if (!empty($member))
			$this->success(array('exist' => 1));
		else
			$this->success(array('exist' => 0));
	}
	
	/**
	 * 找回密码(或者重置密码),前提是用户已经存在,并且已经发送过短信验证码
	 * @param string mobile 手机号
	 * @param string password 密码
	 */
	public function actionReset()
	{
		$mobile = Yii::app()->request->getParam('mobile');
		$isMobile = $this->checkType($mobile);
 		$value = Yii::app()->redis->get($mobile);
 		if (empty($value))
	 		throw new CHttpException(400, '请求错误');
 		
 		if ($isMobile)
			$user = User::model()->findByAttributes(array('mobile' => $mobile));
 		else 
 			$user = User::model()->findByAttributes(array('email' => $mobile));
		if (empty($user))
			$this->failed('用户不存在');
		$password = trim(Yii::app()->request->getParam('password'));
		if (!User::checkPwd($password))
			$this->failed('密码输入错误');
		User::model()->updateByPk($user->id, array('password' => User::model()->encrypting($password)));
		$token = User::model()->doLogin($user);
		$silence = UserCleaner::model()->getSilenceSet($user->id);
		$profile = UserAddress::model()->getFormatInfo($user->id);
		$data = array_merge($silence, $profile);
		$data['token'] = $token;
		$this->success($data);
	}
	
	
	/**
	 * 使用手机号和密码登录
	 * @param string $mobile 手机号
	 * @param string $password 密码
	 * @param sign
	 */
	public function actionLogin()
	{
		$this->addParam();
		$mobile = Yii::app()->request->getParam('mobile');
		$isMobile = $this->checkType($mobile);
		$token = User::model()->login($mobile, Yii::app()->request->getParam('password'));
		$silence = UserCleaner::model()->getSilenceSet(Yii::app()->user->id);
		$profile = UserAddress::model()->getFormatInfo(Yii::app()->user->id);
		$data = array_merge($silence, $profile);
		$data['token'] = $token;
        $data['nickname'] = Yii::app()->user->name;//返回昵称
		$data['user_id'] = Yii::app()->user->id;
		$this->success($data);
	}
	
	/**
	 * 注册
	 * @param string $mobile 手机号
	 * @param string $password 密码
	 * @param string $nickname 昵称
	 * @param string $app_version APP版本号
	 * @param string $sign 签名
	 */
	public function actionRegister()
	{
		$this->addParam();
		$mobile = Yii::app()->request->getParam('mobile');
		$isMobile = $this->checkType($mobile);
		$this->checkSign(array('mobile','password','nickname','app_version','platform','client_id'));
        $isMobile && SmsLog::model()->checkHasSend($mobile);
		$user = new User();
		$user->attributes = array(
			'password' 	  => $_POST['password'],
			'nickname'    => $_POST['nickname'],
			'app_version' => $_POST['app_version']
		);
		if ($isMobile)
        {
            $user->mobile = $mobile;
            $user->is_verify = User::VERIFY_YES;
        }else
        {
            $user->email = $mobile;
            $user->is_verify = User::VERIFY_NO;
        }
		if ($user->save())
		{
			$token = User::model()->doLogin($user);
			$silence = UserCleaner::model()->getSilenceSet(Yii::app()->user->id);
			$profile = UserAddress::model()->getFormatInfo(Yii::app()->user->id);
			$data = array_merge($silence, $profile);
            $data['is_verify'] = $user->is_verify;
			$data['token'] = $token;
			$this->success($data);
		} else
		{
			$error = array_values($user->getErrors());
			$this->failed($error[0][0]);
		}
	}
	
	/**
	 * 兼容之前的版本
	 * @param unknown $mobile
	 * 根据$mobile 判断是手机号 还是 邮箱
	 * @return 如果是手机号 返回true  否则 返回false
	 */
	private function checkType($mobile)
	{
		$isMobile = Util::checkIsMobile($mobile);
		$isEmail = Util::checkIsEmail($mobile);
		if (!$isMobile && !$isEmail)
		{
			throw new CHttpException(400, '请输入正确的账号格式');
		}
		return $isMobile ? true : false;
	}
	
	/**
	 * token失效后重置token
	 * /login/resetToken/token/{token}/sign/{sign}
	 * 
	 */
	public function actionResetToken()
	{
		$this->addParam();
		$this->checkSign(array('token', 'client_id'));
		$origin = Yii::app()->request->getParam('token');
		if (empty($origin))
			$this->failed('缺少参数');
		$data = Token::model()->reset($origin);
		$this->success($data);
	}

	/**
	 * 增加客户端标识参数
	 */
	protected function addParam()
	{
		$_POST['client_id'] = $this->client_id;
	}
}