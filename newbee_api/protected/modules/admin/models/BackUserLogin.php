<?php

/**
 * LoginForm class.
 * LoginForm is the data structure for keeping
 * user login form data. It is used by the 'login' action of 'SiteController'.
 */
class BackUserLogin extends CFormModel
{
	public $username;
	public $password;
	public $rememberMe;
	public $verifyCode;

	/**
	 * Declares the validation rules.
	 * The rules state that username and password are required,
	 * and password needs to be authenticated.
	 */
	public function rules()
	{
		return array(
			// username and password are required
			array('username, password', 'required'),
			// rememberMe needs to be a boolean
			//array('rememberMe', 'boolean'),
			// password needs to be authenticated
			array('password', 'authenticate'),
			//array('verifyCode', 'captcha')
		);
	}

	/**
	 * Declares attribute labels.
	 */
	public function attributeLabels()
	{
		return array(
			'rememberMe'=>'记住我',
			'username'=>'用户名',
			'password'=>'密码',
			'verifyCode'=>'验证码'
		);
	}

	/**
	 * Authenticates the password.
	 * This is the 'authenticate' validator as declared in rules().
	 */
	public function authenticate($attribute,$params)
	{
		if(!$this->hasErrors())  // we only want to authenticate when no input errors
		{
			$identity=new BackUserIdentity($this->username,$this->password);
			$identity->authenticate();
			switch($identity->errorCode)
			{
				case BackUserIdentity::ERROR_NONE:
					$duration=3600;
					Yii::app()->user->login($identity,$duration);
					break;
				case BackUserIdentity::ERROR_USERNAME_INVALID:
					$this->addError("username",'用户名错误');
					break;
				case BackUserIdentity::ERROR_PASSWORD_INVALID:
					$this->addError("password",'密码错误');
					break;
			}
		}
	}
}
