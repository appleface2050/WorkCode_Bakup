<?php

/**
 * 手机端企业登录
 * @author JZLJS00
 *
 */
class BackUserIdentity extends CUserIdentity
{
	
	private $_id;
	
	
	/**
	 * Authenticates a user.
	 * The example implementation makes sure if the username and password
	 * are both 'demo'.
	 * In practical applications, this should be changed to authenticate
	 * against some persistent user identity storage (e.g. database).
	 * @return boolean whether authentication succeeds.
	 */
	public function authenticate()
	{
		$user=Admin::model()->findByAttributes(array('username'=>$this->username));
		if($user===null)
			$this->errorCode=self::ERROR_USERNAME_INVALID;
		else if(Admin::model()->encrypting($this->password)!==$user->password)
			$this->errorCode=self::ERROR_PASSWORD_INVALID;
		else
		{
			$this->_id=$user->id;
			$this->username=$user->username;
			$this->setState('back', true);
			$this->errorCode=self::ERROR_NONE;
		}
		return !$this->errorCode;
	}
	
	/**
	 * @return integer the ID of the user record
	 */
	public function getId()
	{
		return $this->_id;
	}
	
}