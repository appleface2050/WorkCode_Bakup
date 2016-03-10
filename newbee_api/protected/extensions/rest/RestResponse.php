<?php


class RestResponse 
{
	// HTTP Status(状态码)
	const OK                  = 200; // The request has succeeded.
	const CREATED             = 201; // The request has been fulfilled and resulted in a new resource being created.
	const NO_CONTENT          = 204; // The server has fulfilled the request but does not need to return an entity-body.
	const MOVED_PERM          = 301;
	const FOUND               = 302;
	const NOT_MODIFIED        = 304;
	const TEMP_REDERICT       = 307;
	const BAD_REQUEST         = 400; // The request could not be understood by the server due to malformed syntax.
	const UNAUTHORIZED        = 401; // The request requires user authentication.
	const FORBIDDEN           = 403; // The server understood the request, but is refusing to fulfill it.
	const NOT_FOUND           = 404; // The server has not found anything matching the Request-URI
	const NOT_ALLOWED         = 405; // The method specified in the Request-Line is not allowed for the resource identified by the Request-URI
	const NOT_ACCEPTABLE      = 406; // The server can only generate a response that is not accepted by the client
	const REQUEST_TIMEOUT     = 408;
	const CONFLICT			  = 409; 
	const GONE			      = 410;
	const SERVER_ERROR        = 500;
	const NOT_IMPLEMENTED     = 501; // The server does not support the functionality required to fulfill the request.
	const UNAVAILABLE         = 503;
	const TIMEOUT             = 504;
	
	/**
	 * 状态码对应的描述信息
	 * @var unknown
	 */
	public static $statusMsg = array(
		self::OK              => 'OK',
		self::CREATED         => 'Created',
		self::NO_CONTENT      => 'No Content',
		self::MOVED_PERM      => 'Moved Permanently',
		self::FOUND           => 'Found',
		self::NOT_MODIFIED    => 'Not Modified',
		self::TEMP_REDERICT   => 'Temporary Redirect',
		self::BAD_REQUEST     => 'Bad Request',
		self::UNAUTHORIZED    => 'Unauthorized',
		self::FORBIDDEN       => 'Forbidden',
		self::NOT_FOUND       => 'Not Found',
		self::NOT_ALLOWED     => 'Method Not Allowed',
		self::NOT_ACCEPTABLE  => 'Not Acceptable',
		self::REQUEST_TIMEOUT => 'Request Timeout',
		self::CONFLICT	      => 'Conflict',
		self::GONE			  => 'Gone',
		self::SERVER_ERROR    => 'Internal Server Error',
		self::NOT_IMPLEMENTED => 'Not Implemented',
		self::UNAVAILABLE     => 'Service Unavailable',
		self::TIMEOUT         => 'Gateway Timeout'
	);
	
	/**
	 * 状态码
	 * @var array
	 */
	protected $statusCodes = array(
		self::OK              => 'HTTP/1.1 200 OK',
		self::CREATED         => 'HTTP/1.1 201 Created',
		self::NO_CONTENT      => 'HTTP/1.1 204 No Content',
		self::MOVED_PERM      => 'HTTP/1.1 301 Moved Permanently',
		self::FOUND           => 'HTTP/1.1 302 Found',
		self::NOT_MODIFIED    => 'HTTP/1.1 304 Not Modified',
		self::TEMP_REDERICT   => 'HTTP/1.1 307 Temporary Redirect',
		self::BAD_REQUEST     => 'HTTP/1.1 400 Bad Request',
		self::UNAUTHORIZED    => 'HTTP/1.1 401 Unauthorized',
		self::FORBIDDEN       => 'HTTP/1.1 403 Forbidden',
		self::NOT_FOUND       => 'HTTP/1.1 404 Not Found',
		self::NOT_ALLOWED     => 'HTTP/1.1 405 Method Not Allowed',
		self::NOT_ACCEPTABLE  => 'HTTP/1.1 406 Not Acceptable',
		self::REQUEST_TIMEOUT => 'HTTP/1.1 408 Request Timeout',
		self::CONFLICT	      => 'HTTP/1.1 409 Conflict',
		self::GONE			  => 'HTTP/1.1 410 Gone',
		self::SERVER_ERROR    => 'HTTP/1.1 500 Internal Server Error',
		self::NOT_IMPLEMENTED => 'HTTP/1.1 501 Not Implemented',
		self::UNAVAILABLE     => 'HTTP/1.1 503 Service Unavailable',
		self::TIMEOUT         => 'HTTP/1.1 504 Gateway Timeout'
	);
	
	/**
	 * @var int
	*/
	protected $code = self::OK;
	
	/**
	 * 错误信息
	 * @var unknown
	 */
	protected $error = null;
	
	/**
	 * 保存HTTP头信息
	 * @var array
	 */
	protected $headers = array();
	
	/**
	 * 返回数据
	 * @var unknown
	 */
	protected $data = array();
	
	
	/**
	 * @param int $code
	 * @return RestResponse
	 */
	public function setCode($code)
	{
		$this->code = $code;
		return $this;
	}
	
	/**
	 * @return int
	 */
	public function getCode()
	{
		return $this->code;
	}
	
	/**
	 * @param string $message
	 * @param string $type
	 * @return RestResponse
	 */
	public function setError($error)
	{
		$this->error = $error;
		return $this;
	}
	
	/**
	 * @return stdClass
	 */
	public function getError()
	{
		return $this->error;
	}
	
	
	/**
	 * @param stdClass $data
	 * @return void
	 */
	public function setData($data)
	{
		$this->data = $data;
	}
	
	/**
	 * @return stdClass
	 */
	public function getData()
	{
		return $this->data;
	}
	
	
	/**
	 * @param string $string
	 * @return void
	 */
	public function addHeader($string)
	{
		$this->headers[] = $string;
	}
	
	/**
	 * @return array
	 */
	public function getHeaders()
	{
		return $this->headers;
	}
	
	/**
	 * @return void
	 */
	protected function sendHeaders()
	{
		$this->addHeader('Content-type: application/json');
		
		$this->addHeader($this->statusCodes[$this->code]);
		$this->addHeader('Status: '.$this->code);
		
		foreach ($this->getHeaders() as $header)
			header($header);
	}
	
	/**
	 * 接口返回
	 */
	public function send()
	{
		$body = $this->getBody();
		$this->sendHeaders();
		echo $body;
		Yii::app()->end();
	}
	
	/**
	 * 返回的body
	 */
	protected function getBody()
	{
		$result = array();
		$code = $this->getCode();
		if ($code == 200)
		{
			$result['status'] = 1;
			$result['msg'] = 'success';
			$result['value'] = $this->getData();
		} else 
		{
			$result['status'] = 0;
			$result['msg'] = (null !== $this->error) ? $this->error : 'failed';
			$result['value'] = $this->getData();
		}
		return CJSON::encode($result);
	}
}