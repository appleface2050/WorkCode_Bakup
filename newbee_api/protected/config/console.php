<?php

// This is the configuration for yiic console application.
// Any writable CConsoleApplication properties can be configured here.
return CMap::mergeArray(
	array(
		'basePath'=>dirname(__FILE__).DIRECTORY_SEPARATOR.'..',
		'name'=>'净化器',
		'sourceLanguage'=>'zh_cn',
		// preloading 'log' component
		'preload'=>array('log'),
		'import'=>array(
			'application.models.*',
			'application.components.*',
			'ext.redis.EKeys'
		),
		// application components
		'components'=>array(
			'curl' => array(
				'class' => 'ext.ECurl'
			),
			'push' => array(
				'class' => 'ext.push.EPush'
			),
			// 位置相关
			'location' => array(
				'class' => 'ext.location.ELocation'
			),
			'log'=>array(
				'class'=>'CLogRouter',
				'routes'=>array(
					array(
						'class'=>'CFileLogRoute',
						'levels' => 'error',
						'maxFileSize'=>5120, // 5M日志文件大小
						'logFile'=>'console-error.txt',
					),
					array(
						'class'=>'CFileLogRoute',
						'levels'=>'info',
						'categories'=>'application.*',
						'maxFileSize'=>5120, // 5M日志文件大小
						'logFile'=>'console-info.txt',
					),
					array(
						'class'=>'CFileLogRoute',
						'levels'=>'info',
						'categories'=>'push.*',
						'maxFileSize'=>102400, // 5M日志文件大小
						'logFile'=>'push.txt',
					),
				),
			)
		),
	),
	require(dirname(__FILE__).DIRECTORY_SEPARATOR.'console_local.php')
);