<?php

/**
 * Class VerifyController
 * email 安全验证
 */
class VerifyController extends CController
{

	public function actionIndex()
    {
        header('content-type:text/html;charset=utf-8');
        $key = Yii::app()->request->getParam('key');
        if (!$key) {
            //错误提示
            echo '无效的链接';
            die;
        }
        Yii::import('ext.Xencrypt');
        $xen = new Xencrypt();
        $user_id = $xen->decode($key);
        if (!$user_id) {
            //错误提示
            echo '无效的链接';
            die;
        }
        $user = User::model()->findByPk($user_id);
        if (!$user) {
            //用户不存在
            echo '无效的链接';
            die;
        }
        if ($user->is_verify) {
            //已经验证通过
            echo '你的邮箱已经验证通过了！';
            die;
        }
        $user->is_verify = User::VERIFY_YES;
        $user->save();
        echo '恭喜你，已经注册完毕，请在客户端登录使用！';
    }
}