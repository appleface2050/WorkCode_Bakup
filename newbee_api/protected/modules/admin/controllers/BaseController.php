<?php


/**
 * AdminController is the customized base controller class For Admin Module.
 * All controller classes for this application should extend from this base class.
 */
class BaseController extends CController
{
	/**
	 * @var string the default layout for the controller view. Defaults to '//layouts/column1',
	 * meaning using a single column layout. See 'protected/views/layouts/column1.php'.
	 */
	public $layout='column2';

	/**
	 * @var array context menu items. This property will be assigned to {@link CMenu::items}.
	 */
	public $menu=array();
	/**
	 * @var array the breadcrumbs of the current page. The value of this property will
	 * be assigned to {@link CBreadcrumbs::links}. Please refer to {@link CBreadcrumbs::links}
	 * for more details on how to specify this property.
	*/
	public $breadcrumbs=array();


	/**
	 * 操作标题
	 * @var unknown_type
	*/
	public $title = '';


	public $returnUrl = array('default/');

	/**
	 * 登录地址
	 * @var unknown
	*/
	public $loginUrl = array('login/login');


	public function init()
	{
		Yii::app()->setHomeUrl('/admin');
		if(!isset(Yii::app()->user->back))
			$this->redirect($this->loginUrl);
	}
}