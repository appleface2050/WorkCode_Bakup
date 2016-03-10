<?php
/**
 * 错误处理类
 */
class RestErrorHandler extends CErrorHandler
{
	/**
	 * whether to discard any existing page output before error display. Defaults to true.
	 * @var boolean 
	 */
	public $discardOutput = true;
	
	/**
	 * Handles the PHP error.
	 * @param CErrorEvent $event the PHP error event
	 */
	protected function handleError($event)
	{
		$res = new RestResponse();
		$res->setCode(RestResponse::SERVER_ERROR);
		$res->setError($event->message);
		$res->send();
	}
	
	/**
	 * Handles the exception.
	 * @param Exception $exception the exception captured
	 */
	protected function handleException($exception)
	{
		$code = ($exception instanceof CHttpException) ? $exception->statusCode : RestResponse::SERVER_ERROR;
		$message = $exception->getMessage();
		empty($message) && ($message = RestResponse::$statusMsg[$code]);
		$res = new RestResponse();
		$res->setCode($code);
		$res->setError($message);
		$res->send();
	}
}