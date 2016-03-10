# 2015-09-15
#user ������һ���ֶ�  credit ����
ALTER TABLE `user` ADD COLUMN `credit` INT UNSIGNED DEFAULT 0 NOT NULL COMMENT '����' AFTER `is_verify`;
#���ּ�¼��
CREATE TABLE `credit_log` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT COMMENT '���ּ�¼��',
  `user_id` int(11) NOT NULL COMMENT '�û�id',
  `action_id` tinyint(1) NOT NULL COMMENT '1 ��½ 2 ���� 3 �����쿪�� 4 �һ�',
  `type_id` tinyint(4) NOT NULL COMMENT '���� 1 �� 2 ��',
  `credit` smallint(6) NOT NULL COMMENT '����|��� ������',
  `credit_info` varchar(100) NOT NULL COMMENT '��������',
  `create_date` date NOT NULL COMMENT '����',
  `create_time` int(10) NOT NULL COMMENT '����ʱ��',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8;

#��Ӿ����������
CREATE TABLE `user_apply_log` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `apply_uid` int(10) unsigned NOT NULL COMMENT '�û�id',
  `apply_tid` int(10) unsigned NOT NULL DEFAULT '0' COMMENT '�������û�id(���������û����������)',
  `cleaner_id` varchar(100) NOT NULL COMMENT '������id',
  `verify_uid` int(10) unsigned NOT NULL DEFAULT '0' COMMENT 'ִ����˲������û�id',
  `add_time` int(10) unsigned NOT NULL COMMENT '����ʱ��',
  `status` tinyint(1) unsigned NOT NULL DEFAULT '1' COMMENT '1������2ͨ��3�ܾ�',
  `note` varchar(50) NOT NULL DEFAULT '' COMMENT '����ʱ��ı�ע',
  `verify_time` int(10) unsigned NOT NULL DEFAULT '0' COMMENT '���ʱ��',
  `is_delete` tinyint(1) NOT NULL DEFAULT '0' COMMENT '�Ƿ���ɾ��0δɾ��1��ɾ��',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
#��������о�������    cleaner_filter_change,task,weixin_user,`filter_exchange`,`weixin_share`

CREATE TABLE `cleaner_filter_change` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT COMMENT '��о������¼��',
  `cleaner_id` varchar(50) NOT NULL,
  `filter_id` tinyint(4) NOT NULL,
  `reset_time` int(10) NOT NULL COMMENT '����ʱ��',
  PRIMARY KEY (`id`),
  UNIQUE KEY `cleaner_id` (`cleaner_id`,`filter_id`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8;


CREATE TABLE `weixin_user` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `weixin_openid` varchar(50) CHARACTER SET utf8 NOT NULL DEFAULT '' COMMENT '΢��openid',
  `username` varchar(60) NOT NULL COMMENT '΢���ǳ�',
  `headimgurl` varchar(300) CHARACTER SET utf8 DEFAULT '' COMMENT '΢��ͷ��',
  `sex` tinyint(1) NOT NULL DEFAULT '0' COMMENT '�Ա�',
  `add_time` int(10) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `weixin_openid` (`weixin_openid`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4;

CREATE TABLE `weixin_share` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `user_id` int(10) unsigned NOT NULL,
  `cleaner_id` char(50) NOT NULL,
  `filter_id` tinyint(1) NOT NULL,
  `date` int(10) unsigned NOT NULL COMMENT '��һ������',
  `add_time` int(10) unsigned NOT NULL COMMENT '����ʱ��',
  PRIMARY KEY (`id`),
  KEY `cleaner_id` (`cleaner_id`,`filter_id`,`add_time`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8 COMMENT='�һ���о΢�ŷ���';

CREATE TABLE `filter_exchange` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `user_id` int(11) NOT NULL,
  `cleaner_id` varchar(100) NOT NULL COMMENT '������id',
  `filter_id` tinyint(4) NOT NULL COMMENT '��оid',
  `filter_name` varchar(100) NOT NULL DEFAULT '' COMMENT '��о����',
  `create_time` int(10) NOT NULL COMMENT '����ʱ��',
  `receiver_name` varchar(30) NOT NULL,
  `receiver_mobile` varchar(11) NOT NULL,
  `receiver_address` varchar(100) NOT NULL,
  `remark` varchar(300) NOT NULL DEFAULT '' COMMENT '��ע',
  `shipping_info` varchar(300) NOT NULL DEFAULT '' COMMENT '������Ϣ',
  PRIMARY KEY (`id`),
  KEY `cleaner_id` (`cleaner_id`,`filter_id`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8;