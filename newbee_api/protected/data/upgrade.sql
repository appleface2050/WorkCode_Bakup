# 2015-09-15
#user 表增加一个字段  credit 积分
ALTER TABLE `user` ADD COLUMN `credit` INT UNSIGNED DEFAULT 0 NOT NULL COMMENT '积分' AFTER `is_verify`;
#积分记录表
CREATE TABLE `credit_log` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT COMMENT '积分记录表',
  `user_id` int(11) NOT NULL COMMENT '用户id',
  `action_id` tinyint(1) NOT NULL COMMENT '1 登陆 2 分享 3 雾霾天开机 4 兑换',
  `type_id` tinyint(4) NOT NULL COMMENT '类型 1 加 2 减',
  `credit` smallint(6) NOT NULL COMMENT '消耗|获得 积分数',
  `credit_info` varchar(100) NOT NULL COMMENT '积分描述',
  `create_date` date NOT NULL COMMENT '日期',
  `create_time` int(10) NOT NULL COMMENT '创建时间',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8;

#添加净化器申请表
CREATE TABLE `user_apply_log` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `apply_uid` int(10) unsigned NOT NULL COMMENT '用户id',
  `apply_tid` int(10) unsigned NOT NULL DEFAULT '0' COMMENT '被申请用户id(存在向多个用户申请的问题)',
  `cleaner_id` varchar(100) NOT NULL COMMENT '净化器id',
  `verify_uid` int(10) unsigned NOT NULL DEFAULT '0' COMMENT '执行审核操作的用户id',
  `add_time` int(10) unsigned NOT NULL COMMENT '申请时间',
  `status` tinyint(1) unsigned NOT NULL DEFAULT '1' COMMENT '1申请中2通过3拒绝',
  `note` varchar(50) NOT NULL DEFAULT '' COMMENT '申请时候的备注',
  `verify_time` int(10) unsigned NOT NULL DEFAULT '0' COMMENT '审核时间',
  `is_delete` tinyint(1) NOT NULL DEFAULT '0' COMMENT '是否已删除0未删除1已删除',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
#净化器滤芯更换相关    cleaner_filter_change,task,weixin_user,`filter_exchange`,`weixin_share`

CREATE TABLE `cleaner_filter_change` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT COMMENT '滤芯更换记录表',
  `cleaner_id` varchar(50) NOT NULL,
  `filter_id` tinyint(4) NOT NULL,
  `reset_time` int(10) NOT NULL COMMENT '重置时间',
  PRIMARY KEY (`id`),
  UNIQUE KEY `cleaner_id` (`cleaner_id`,`filter_id`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8;


CREATE TABLE `weixin_user` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `weixin_openid` varchar(50) CHARACTER SET utf8 NOT NULL DEFAULT '' COMMENT '微信openid',
  `username` varchar(60) NOT NULL COMMENT '微信昵称',
  `headimgurl` varchar(300) CHARACTER SET utf8 DEFAULT '' COMMENT '微信头像',
  `sex` tinyint(1) NOT NULL DEFAULT '0' COMMENT '性别',
  `add_time` int(10) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `weixin_openid` (`weixin_openid`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4;

CREATE TABLE `weixin_share` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `user_id` int(10) unsigned NOT NULL,
  `cleaner_id` char(50) NOT NULL,
  `filter_id` tinyint(1) NOT NULL,
  `date` int(10) unsigned NOT NULL COMMENT '哪一天分享的',
  `add_time` int(10) unsigned NOT NULL COMMENT '分享时间',
  PRIMARY KEY (`id`),
  KEY `cleaner_id` (`cleaner_id`,`filter_id`,`add_time`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8 COMMENT='兑换滤芯微信分享';

CREATE TABLE `filter_exchange` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `user_id` int(11) NOT NULL,
  `cleaner_id` varchar(100) NOT NULL COMMENT '净化器id',
  `filter_id` tinyint(4) NOT NULL COMMENT '滤芯id',
  `filter_name` varchar(100) NOT NULL DEFAULT '' COMMENT '滤芯名称',
  `create_time` int(10) NOT NULL COMMENT '创建时间',
  `receiver_name` varchar(30) NOT NULL,
  `receiver_mobile` varchar(11) NOT NULL,
  `receiver_address` varchar(100) NOT NULL,
  `remark` varchar(300) NOT NULL DEFAULT '' COMMENT '备注',
  `shipping_info` varchar(300) NOT NULL DEFAULT '' COMMENT '发货信息',
  PRIMARY KEY (`id`),
  KEY `cleaner_id` (`cleaner_id`,`filter_id`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8;