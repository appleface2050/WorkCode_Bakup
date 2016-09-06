SET FOREIGN_KEY_CHECKS = 0;

DROP TABLE IF EXISTS  `game_library`;

CREATE TABLE `game_library` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `game_name` varchar(64) NOT NULL,
  `type` varchar(32) DEFAULT NULL,
  `platformId` int(11) NOT NULL,
  `platform_game_id` bigint(20) NOT NULL,
  `icon_url` varchar(1024) DEFAULT NULL,
  `size` bigint(20) DEFAULT NULL,
  `download_count` bigint(20) DEFAULT NULL,
  `modify_date` datetime DEFAULT NULL,
  `screenshots` text,
  `version` varchar(32) DEFAULT NULL,
  `tags` varchar(512) DEFAULT NULL,
  `level` int(11) DEFAULT NULL,
  `instruction` varchar(2048) DEFAULT NULL,
  `description` text,
  `deleted` int(11) DEFAULT '0',
  `bs_deleted` int(11) DEFAULT '0',
  `uptime` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `index_game_name` (`game_name`) USING BTREE,
  KEY `index_` (`platformId`,`platform_game_id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=13838 DEFAULT CHARSET=utf8;


DROP TABLE IF EXISTS  `platform`;
CREATE TABLE `platform` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `platform_name` varchar(32) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8;

insert into `platform`(`id`,`platform_name`) values
('1','game9'),
('2','baidu'),
('3','360');
SET FOREIGN_KEY_CHECKS = 1;

