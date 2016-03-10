<?php

/**
 * 其它函数的基类(客户端参数问题通过throw exception形势返回)
 * @author zhoujianjun
 */
class RestController extends CController
{
	
	/**
	 * 加密的key
	 * @var unknown
	 */
	protected $key = '#$cleaner&*%$app';
	
	/**
	 * 已登录用户token
	 * @var unknown
	 */
	protected $token = '';
	
	/**
	 * token对应的用户
	 * @var unknown
	 */
	protected $_user = null;
	
	
	/**
	 * 客户端标识
	 * @var unknown
	 */
	protected $client_id = 'AIRCLEANERNEEBEER';
	
	
	/**
	 * 设置ErrorHandler(non-PHPdoc)
	 * @see CController::init()
	 */
	public function init()
	{
		$errorHandler = Yii::createComponent(array('class'=>'RestErrorHandler'));
		Yii::app()->setComponent('errorHandler', $errorHandler);
	}
	
	
	/**
	 * (non-PHPdoc) 检测用户是否合法
	 * @see CController::beforeAction()
	 */
	public function beforeAction($action)
	{
		$token = Yii::app()->request->getParam('token','');
		if(empty($token))
			throw new CHttpException(403, '缺少token参数');
		$model = Token::model()->with('user')->findByAttributes(array('token'=>$token));
		if(empty($model) || empty($model->user))
			throw new CHttpException(403, 'token参数错误');
		/*if ((time()-$model->add_time) > Token::LIFETIME)
			throw new CHttpException(400, 'token已经过期了');*/
		$user = $model->user;
        if(!$user->is_verify)
            throw new CHttpException(403, '您的账号还没有通过email验证，暂时不能使用');
		$this->_user = $user;
		Yii::app()->user->id = $user->id;
		Yii::app()->user->name = $user->nickname;
		Yii::app()->user->setState('is_admin', $user->is_admin);
		$this->token = $token;

		// 记录访问日志
// 		$controllerId = explode('/', $this->getId());
// 		$actionId = $action->getId();
// 		$log = new AccessLog();
// 		$log->attributes = array(
// 			'user_id' => Yii::app()->user->id,
// 			'controller' => $controllerId[1],
// 			'action' => $actionId,
// 			'date' => strtotime(date('Y-m-d')),
// 			'add_time' => time()
// 		);
// 		$log->save();
		
		// 加入集合
		Yii::app()->redis->sAdd(EKeys::getActiveUserKey(), Yii::app()->user->id);

		return true;
	}

	
	/**
	 * 操作成功的返回,
	 * @param unknown $data
	 */
	protected function success($data=array())
	{
		$res = new RestResponse();
		$res->setCode(RestResponse::OK);
		$res->setData($data);
		$res->send();
	}
	
	/**
	 * 失败的返回
	 * @param unknown $error 错误信息
	 */
	protected function failed($error)
	{
		$res = new RestResponse();
		$res->setCode(RestResponse::SERVER_ERROR);
		$res->setError($error);
		$res->send();
	}
	
	/**
	 * 对于POST请求验证签名
	 * @param $key 要进行签名的keys
	 * 
	 */
	protected function checkSign($keys=array())
	{
		$data = array();
		foreach ($keys as $key)
		{
			$data[$key] = Yii::app()->request->getParam($key, '');
		}
		ksort($data);
		
		$join = array();
		foreach ($data as $key => $value)
		{
			$join[] = "$key=$value";
		}
		if(!(md5(join('&', $join) . $this->key) == Yii::app()->request->getParam('sign')))
			throw new CHttpException(400, '签名错误');
		return true;
	}
	
	/**
	 * 检测某个净化器是否在线
	 * @param unknown $id 净化器id
	 * @return boolean
	 */
	protected function checkOnline($id)
	{
		$key = EKeys::getAllonlineKey();
		return Yii::app()->redis->sIsMember($key, $id) ? true : false;
	}
	
	/**
	 * 净化器是否在线升级
	 * @param unknown $id
	 * @return 在线升级 则返回true  不是在线升级 则返回false
	 */
	protected function checkUpgrade($id)
	{
		$key = EKeys::getUpgradeKey();
		return Yii::app()->redis->sIsMember($key, $id) ? true : false;
	}
	
	/**
	 * 增加客户端标识参数
	 */
	protected function addParam()
	{
		$_POST['client_id'] = $this->client_id;
	}

	protected function getUser()
	{
		return $this->_user;
	}

	/**
	 * 特殊类型数据返回
	 * @param $resultCode
	 * @param string $resultMsg
	 * @param array $resultData
	 */
	protected function specialResponse($resultCode,$resultMsg='',$resultData=array())
	{
		header('Content-type: application/json');
		header('HTTP/1.1 200 OK');
		$data['status']  = $resultCode;
		$data['msg']     = $resultMsg;
		$data['value']   = $resultData;
		echo CJSON::encode($data);
		Yii::app()->end();
	}
}