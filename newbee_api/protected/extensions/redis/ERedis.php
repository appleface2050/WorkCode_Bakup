<?php

/**
 * redis相关
 * @author zhoujianjun
 *
 */
class ERedis extends CApplicationComponent
{
	
	/**
	 * redis host
	 * @var unknown
	 */
	public $host = null;
	
	/**
	 * redis port
	 * @var unknown
	 */
	public $port = 6379;
	
	
	/**
	 * 数据库
	 * @var unknown
	 */
	public $db = 0;
	
	
	/**
	 * redis 连接实例
	 * @var unknown
	 */
	protected $connention = null;
	
	/**
	 * 初始化连接(non-PHPdoc)
	 * @see CApplicationComponent::init()
	 */
	public function init()
	{
		try {
			$connention = new Redis();
			$connention->connect($this->host, $this->port);
			$connention->select($this->db);
			$this->connention = $connention;
		} catch (Exception $e) {
			throw new CException('Redis not installed');
		}
	}
	
	
	/**
	 * Calls the named method which is not a class method.
	 * Do not call this method. This is a PHP magic method that we override
	 * to implement the behavior feature.
	 * @param string $name the method name
	 * @param array $parameters method parameters
	 * @throws CException if current class and its behaviors do not have a method or closure with the given name
	 * @return mixed the method return value
	 */
	public function __call($name,$parameters)
	{
		return call_user_func_array(array($this->connention, $name), $parameters);	
	}
	
	/**
	 * 关闭连接
	 */
	public function close()
	{
		$this->connention->close();
	}
	
}