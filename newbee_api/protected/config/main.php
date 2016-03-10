<?php
Yii::setPathOfAlias('bootstrap', dirname(__FILE__).'/../extensions/bootstrap');
// uncomment the following to define a path alias
// Yii::setPathOfAlias('local','path/to/local-folder');

// This is the main Web application configuration. Any writable
// CWebApplication properties can be configured here.

return CMap::mergeArray(
	array(
		'basePath'=>dirname(__FILE__).DIRECTORY_SEPARATOR.'..',
		'name'=>'净化器',
		'sourceLanguage'=>'zh_cn',
		// preloading 'log' component
		'preload'=>array('log'),
	
		// autoloading model and component classes
		'import'=>array(
			'application.models.*',
			'application.components.*',
			'application.modules.admin.controllers.AdminController',
			'ext.rest.RestErrorHandler',
			'ext.rest.RestResponse',
			'ext.redis.EKeys'
		),
	
		'modules'=>array('admin'),
	
		// application components
		'components'=>array(
			'bootstrap'=>array(
				'class'=>'bootstrap.components.Bootstrap',
			),
			
			'user'=>array(
				// enable cookie-based authentication
				'allowAutoLogin'=>true,
			//	'autoRenewCookie'=>true,
				'loginUrl'=>array('/admin/login/login'),
				//'returnUrl'=>'/record/check',
			),
			// uncomment the following to enable URLs in path-format
			
			'urlManager'=>array(
				'urlFormat'=>'path',
				'showScriptName'=>false,
				'rules' => array(
						//'/manage'=>'/admin/default/index',
						'manage'=> '/admin/login/login'
				)
			),
			'push' => array(
					'class' => 'ext.push.EPush'
			),
			// uncomment the following to use a MySQL database
			
			// redis订阅相关
			'publish' => array(
				'class' => 'ext.redis.EPublish'
			),
			
			// 位置相关
			'location' => array(
				'class' => 'ext.location.ELocation'
			),
			
			'curl' => array(
				'class' => 'ext.ECurl'
			),
			
			// send email
			'mail' => array(
				'class' => 'ext.Mail.Mail',
			),
			
			'sms' => array(
				'class' => 'ext.Sms'
			),
			'errorHandler'=>array(
				// use 'site/error' action to display errors
				'errorAction'=>'site/error',
			),
			'log'=>array(
				'class'=>'CLogRouter',
				'routes'=>array(
					array(
						'class'=>'CFileLogRoute',
						'levels' => 'error',
						'maxFileSize'=>5120, // 5M日志文件大小
						'logFile'=>'error.txt',
					),
					array(
						'class'=>'CFileLogRoute',
						'levels'=>'info',
						'categories'=>'application.*',
						'maxFileSize'=>5120, // 5M日志文件大小
						'logFile'=>'info.txt',
					),
				),
			),
		),
	
		// application-level parameters that can be accessed
		// using Yii::app()->params['paramName']
		'params'=>array(
			// this is used in contact page
			'adminEmail'=>'webmaster@example.com',
			'weixin'=>array(
				'token'=>'A786A0EF093EE935A832FC56E8641A06',
				'appId' => 'wxea1e47cc2b6988cb',
				'appSecret'=>'bd9cb8293770659e2a14819e54a012ff',
				'aeskey' => 'oRPTw8dM4Oprvk2azyIR33j7woKn1EpRMG6LW5FUEbr'
			)
		),
	),
	require(dirname(__FILE__).DIRECTORY_SEPARATOR.'local.php')
);