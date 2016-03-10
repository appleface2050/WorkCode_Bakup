CREATE TABLE `share_text` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT COMMENT '分享文本表',
  `title` varchar(100) NOT NULL COMMENT '标题',
  `content` varchar(200) NOT NULL COMMENT '内容',
  `image_path` varchar(50) NOT NULL COMMENT '图片路径',
  `page_url` varchar(100) NOT NULL COMMENT '页面url',
  `create_time` int(10) NOT NULL COMMENT '创建时间',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8
