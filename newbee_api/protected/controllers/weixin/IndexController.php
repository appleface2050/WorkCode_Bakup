<?php

/**
 * 与微信交互
 * @author JZLJS00
 *
 */
class IndexController extends RestController
{
    /**
     * 微信端发送的数据
     * @var array
     */
    private $_data = array();

    /**
     * CLICK事件回复文字的key
     * @var unknown
     */
    private $textResponseKey = array(
        'SERVICE_PHONE',
    );

    /**
     * CLICK事件回复图文的key
     */
    private $textImgResponseKey = array(
    );

    /**
     * 文字响应回复文子的key
     */
    private $textResponseInputKey = array(
        '游戏'
    );

    /**
     * 文字响应回复文子的key
     */
    private $textImgResponseInputKey = array(
        '三个爸爸'
    );

    /**
     * 重载父的init(non-PHPdoc)
     * @see CController::init()
     */
    public function init()
    {

    }

    /**
     * 验证消息的合法性
     * @see CController::beforeAction()
     */
    public function beforeAction($action)
    {
        $echoStr = Yii::app()->request->getParam('echostr','');
        if(!empty($echoStr))
        {
            $request = Yii::app()->request;
            $signature = $request->getParam('signature','');
            $tmpArr = array(Yii::app()->params['weixin']['token'], $request->getParam('timestamp',''), $request->getParam('nonce',''));
            sort($tmpArr);
            if(sha1(implode($tmpArr)) == $signature)
            {
                echo $echoStr;exit();
            }
        }
        return true;
    }

    /**
     * 获取微信端发送的数据
     */
    public function actionIndex()
    {
        $data = file_get_contents("php://input");
        if (!empty($data))
        {
            $this->_data = json_decode(json_encode(simplexml_load_string($data, 'SimpleXMLElement', LIBXML_NOCDATA)),true);
            $this->dispatch();
        }
    }

    /**
     * 微信授权
     */
    public function actionAuth(){
        if(isset($_SESSION['authcode'])){
            unset($_SESSION['authcode']);
        }
        $encrypt_code = trim(Yii::app()->request->getParam('encrypt_code'));
        $returnUrl = urlencode($this->createAbsoluteUrl('/weixin/index/Return/encrypt_code/'.$encrypt_code));
        $url = 'https://open.weixin.qq.com/connect/oauth2/authorize?appid='.Yii::app()->params['weixin']['appId'].'&redirect_uri='.$returnUrl.'&response_type=code&scope=snsapi_userinfo&state=sangebabacleaner#wechat_redirect';
        Yii::app()->request->redirect($url);
    }

    /**
     * 微信授权返回
     */
    public function actionReturn(){
        $state = Yii::app()->request->getParam('state');
        if($state == 'sangebabacleaner')
        {
            $code = Yii::app()->request->getParam('code');
            if(!isset($_SESSION['authcode']))
            {
                $_SESSION['authcode'] = $code;
                $url = 'https://api.weixin.qq.com/sns/oauth2/access_token?appid='.Yii::app()->params['weixin']['appId'].'&secret='.Yii::app()->params['weixin']['appSecret'].'&code='.$code.'&grant_type=authorization_code';
                $result = Yii::app()->curl->get($url);
                $result = json_decode($result,true);
                if(isset($result['access_token'],$result['openid']))
                {
                    $url = 'https://api.weixin.qq.com/sns/userinfo?access_token='.$result['access_token'].'&openid='.$result['openid'];
                    $return = Yii::app()->curl->get($url);
                    $return = json_decode($return,true);
                    if(isset($return['openid']))
                    {
                        //$return['nickname'] = 'no_name';//json_decode($tmpStr);
                        $return['encrypt_code'] = Yii::app()->request->getParam('encrypt_code');
                        $this->saveWxUserInfo($return);
                    }else{
                        /*print_r($result);
                        echo 'here';*/
                        header('content-type:text/html;charset=utf-8');
                        echo '授权失败';
                        //$this->redirect('/weixin/auth/brand_id/'.$brand_id);
                    }
                }
            }else{
                Yii::log('repeat request','info');
            }
        }else{
            unset($_SESSION['authcode']);
            echo 'error auth';
        }
    }

    /**
     *
     */

    /**
     * 保存微信用户信息
     */
    private function saveWxUserInfo($param){
        if(empty($param)){
            return false;
        }
        $model = UserWeixin::model()->findByAttributes(array('weixin_openid'=> $param['openid']));
        if(!$model){
            $model = new UserWeixin();
            $param['nickname'] = preg_replace('/[\x{10000}-\x{10FFFF}]/u', '', $param['nickname']);
            $model->attributes = array(
                'weixin_openid' => $param['openid'],
                'username'  => $param['nickname'],
                'headimgurl' => $param['headimgurl'],
                'sex' => $param['sex'],
                'add_time' => time()
            );
            if(!$model->save())
            {
                var_dump($model->getErrors());
                echo 'error auth 001';
                die;
            }
        }
        $from_url = Yii::app()->request->hostInfo.'/change_filter/send.html';
        $from_url .= '?encrypt_code='.$param['encrypt_code'].'&wx_id=123bdfdfsasr323fgg';
        //设置cookie 保存微信id
        $cookie = new CHttpCookie('wx_id',$model->weixin_openid);
        Yii::app()->request->cookies['wx_id']=$cookie;
        $this->redirect($from_url);
    }

    /**
     * 根据$_data数据的不同,调用不同的函数
     */
    protected function dispatch()
    {
        $msgType = strtolower($this->_data['MsgType']);
        $action = '';
        switch ($msgType)
        {
            case 'text':
                if(in_array($this->_data['Content'], $this->textResponseInputKey)){
                    $action = 'handelResponseMsg';
                }elseif(in_array($this->_data['Content'], $this->textImgResponseInputKey)){
                    $action = 'handelResponseImgMsg';
                }else{
                    $action = 'handelResponseDefaultMsg';
                }
                break;
            case 'event':
                $map = array('subscribe' => 'handleSubscribeEvent','unsubscribe' => 'handleUnSubscribeEvent', 'CLICK' => 'handleClickEvent');
                $action = isset($map[$this->_data['Event']]) ? $map[$this->_data['Event']] : '';
                if(empty($action)){
                    exit;
                }
                break;
            default:
                $action = 'handelResponseDefaultMsg';
                break;
        }
        if(!empty($action))
            $this->$action();
    }

    /**
     * 接收文本消息,回复图文消息
     */
    protected function handelResponseImgMsg()
    {
        echo WeixinBridge::responseTextImgMessage($this->_data['ToUserName'], $this->_data['FromUserName'], Weixin::handelTextImgMsg($this->_data));
    }

    /**
     * 接收文本消息,回复文字消息
     */
    protected function handelResponseMsg()
    {
        echo WeixinBridge::responseTextMessage($this->_data['ToUserName'], $this->_data['FromUserName'], Weixin::handelTextMsg($this->_data));
    }

    /**
     * 回复客服默认的消息
     */
    protected  function handelResponseDefaultMsg(){
        $str = WeixinBridge::responseTransferCustomerService($this->_data['ToUserName'], $this->_data['FromUserName']);
        echo $str;
    }

    /**
     * 关注公众号事件
     */
    protected function handleSubscribeEvent()
    {
        echo WeixinBridge::responseTextMessage($this->_data['ToUserName'], $this->_data['FromUserName'], Weixin::handleSubscribeEvent($this->_data));
    }

    /**
     * 取消关注公众号事件
     */
    /* protected function handleUnSubscribeEvent()
    {
        Weixin::handleUnSubscribeEvent($this->_data);
    } */

    /**
     * 点击自定义菜单事件
     */
    protected function handleClickEvent()
    {
        if(in_array($this->_data['EventKey'], $this->textImgResponseKey))
            echo WeixinBridge::responseTextImgMessage($this->_data['ToUserName'], $this->_data['FromUserName'], Weixin::handleClickEvent($this->_data));
        elseif(in_array($this->_data['EventKey'], $this->textResponseKey))
            echo WeixinBridge::responseTextMessage($this->_data['ToUserName'], $this->_data['FromUserName'], Weixin::handleClickEvent($this->_data));
    }


    /**
     * 创建菜单
     */
    public function actionCreateMenu()
    {
        return WeixinBridge::createMenu(Weixin::getMenus());
    }

    /**
     * 删除菜单
     */
    public function actionDeleteMenu()
    {
        WeixinBridge::deleteMenu();
    }

}