<?php
/**
 * 发邮件
 * @author JZLJS00
 *
 */
class Mail extends CApplicationComponent
{
	
	/**
	 * true 使用163服务器, false 使用sendmail
	 * @var bool
	 */
	private $_debug = true;
	
	/**
	 * 默认配置
	 * @var unknown_type
	 */
	private $_config = array(
		'CharSet' => 'UTF-8',
		'Port' => 25,
		'FromName' => '三个爸爸',
	);
	
	private $_mailer = null;
	
	/**
	 * 
	 */
	public function __construct()
	{
		require __DIR__.'/PHPMailer/PHPMailerAutoload.php';
	}
	
	
	public function init()
	{
		$this->_mailer = new PHPMailer();
		if($this->_debug)
		{
			$this->_config['SMTPAuth'] = true;
			$this->_config['Host'] = 'smtp.exmail.qq.com';
			$this->_config['Username'] = 'register@newbeeair.com';
			$this->_config['Password'] = 'newbee@123';
			$this->_config['From'] = 'register@newbeeair.com';
			$this->_mailer->IsSMTP();
		}else{
// 			$this->_config['From'] = 'service@mail.wanglai.cc';
// 			$this->_mailer->IsSMTP();
		}
		$this->_mailer->IsHTML(true);
		foreach ($this->_config as $key => $value)
			$this->_mailer->$key = $value;
	}
	
	
	public function send($email, $title,$content)
	{
		if(is_array($email))
		{
			foreach ($email as $e)
			{
				$this->_mailer->AddAddress($e);
			}
		}else{
			$this->_mailer->AddAddress($email);
		}
		$this->_mailer->Subject = $title;
		$this->_mailer->Body = $content;
		return $this->_mailer->Send() ? true : false;
	}
	
	public function getError()
	{
		return $this->_mailer->ErrorInfo;
	}
	
}