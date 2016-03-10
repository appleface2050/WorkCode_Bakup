<?php

/**
 * 个人信息管理(修改密码等操作)
 * @author zhoujianjun
 *
 */

class ProfileController extends RestController
{
	
	/**
	 * 更改密码
	 * @param string token
	 * @param string  origin_pwd
	 * @param string  new_pwd
	 */
	public function actionChangePassword()
	{
		$origin_pwd = Yii::app()->request->getParam('origin_pwd');
		$new_pwd = Yii::app()->request->getParam('new_pwd');
		if (User::model()->encrypting($origin_pwd) != $this->_user->password)
			throw new CHttpException(400, '原密码错误');
		$this->_user->password = User::model()->encrypting($new_pwd);
		$this->_user->save();
		$this->success();
	}
	
	
	/**
	 * 退出登录
	 * @method GET
	 * @param string $tokn 用户token
	 */
	public function actionLogout()
	{
		Token::model()->destoryToken($this->token);
		$this->success();
	}
	
	/**
	 * @method POST 
	 * @param name
	 * @param mobile
	 * @param provinceId
	 * @param provinceName
	 * @param cityId
	 * @param cityName
	 * @param address
	 */
	public function actionAddress()
	{
		$model = new UserAddress();
		$model->attributes = $_POST;
		$model->user_id = Yii::app()->user->id;
		if ($model->save())
		{
			$this->success();
		} else {
			$error = array_values($model->getErrors());
			$this->failed($error[0][0]);
		}
	}
	
	/**
	 * 更改Address里的信息
	 */
	public function actionUpdate()
	{
		$model = UserAddress::model()->findByAttributes(array('user_id'=>Yii::app()->user->id));
		if (empty($model))
		{
			$model = new UserAddress();
			$model->user_id = Yii::app()->user->id;
		}
		foreach ($_POST as $key => $value)
		{
			if (!in_array($key, array('token','sign')))
				$model->$key = $value;
		}
		if ($model->save())
		{
			$this->success();
		} else {
			$error = array_values($model->getErrors());
			$this->failed($error[0][0]);
		}
	}
	
	/**
	 * 获取全局的静音时间设置,如果没有净化器则返回默认的静音设置
	 * @param string token
	 */
	public function actionSilenceSet()
	{
		$silence = UserCleaner::model()->getSilenceSet(Yii::app()->user->id);
		$this->success($silence);
	}
	
	/**
	 * 个人信息
	 */
	public function actionIndex()
	{
		$silence = UserCleaner::model()->getSilenceSet(Yii::app()->user->id);
		$profile = UserAddress::model()->getFormatInfo(Yii::app()->user->id);
		$data = array_merge($silence, $profile);
        $data['nickname'] = Yii::app()->user->name;//返回昵称
		$this->success($data);
	}

    /**
     * 修改用户昵称
     * @param string token
     * @param string nickname
     */
    public function actionChangeNickname()
    {
        $nickname = trim(Yii::app()->request->getParam('nickname'));
        if(!$nickname)
            throw new CHttpException(400, '昵称不能为空');
        if(strlen($nickname)<3)
            throw new CHttpException(400, '昵称太短了');
        $this->_user->nickname = $nickname;
        if($this->_user->save())
        {
            $this->success();
        }else
        {
            $error = array_values($this->_user->getErrors());
            $this->failed($error[0][0]);
        }
    }
}