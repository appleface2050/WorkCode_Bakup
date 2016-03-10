<?php

class EKeys extends CApplicationComponent
{
	
	/**
	 * 获取缓存的key,即为redis中缓存的每个净化器的信息,redis的数据结构是hash
	 * @param unknown $id  净化器id
	 */
	public static function getCleanerCacheKey($id)
	{
		return 'CLEANER:ID:' . $id;
	}
	
	/**
	 * 所有当前已联网的机器的id的集合
	 */
	public static function getAllonlineKey()
	{
		return 'ALLONLINE';
	}
	
	/**
	 * 在线升级的净化器(集合 元素为所有在客户端请求升级的净化器id的集合,在php端集合增加元素,在nodejs云端升级成功后,集合里删除元素)
	 */
	public static function getUpgradeKey()
	{
		return 'UPGRADE:ALL';
	}
	
	/**
	 * 升级程序(redis 的hash存储  client_id => 升级程序绝对地址)
	 * @return string
	 */
	public static function getUpgradeProgramKey()
	{
		return 'UPGRADE:PROGRAM';
	}
	
	/**
	 * 升级过程(redis的hash  cleaner_id => 已发送多少帧)
	 */
	public static function getUpgradeFlowKey()
	{
		return 'UPGRADE:FLOW';
	}
	
	/**
	 * 获取任何连过网的key
	 * @param $yesterday 是返回昨天的 还是今天的, 默认返回今天的
	 */
	public static function getHistoryKey($yesterday=false)
	{
		if ($yesterday)
			return 'HISTORYLINE-' . date('Y-n-j', strtotime('-1 day'));
		else
			return 'HISTORYLINE-' . date('Y-n-j');
	}
	
// 	/**
// 	 * 获取存储档位信息的集合(set)的key,每5分钟发送
// 	 */
// 	public static function getLevelKey()
// 	{
// 		$date = getdate();
// 		$minutes = $date['minutes'] < 30 ? 0 : 30;
// 		return 'LEVEL-' . $date['year'] . '-' . $date['mon'] . '-' . $date['mday'] . '-' . $date['hours'] . '-' . $minutes;
// 	}
	
// 	/**
// 	 * 获取每5分钟发送的pm采样数据的key(set)
// 	 */
// 	public static function getRecordKey()
// 	{
// 		$date = getdate();
// 		$minutes = $date['minutes'] < 30 ? 0 : 30;
// 		return 'RECORD-' . $date['year'] . '-' . $date['mon'] . '-' . $date['mday'] . '-' . $date['hours'] . '-' . $minutes;
// 	}
	
	/**
	 * 活跃用户key(集合)
	 */
	public static function getActiveUserKey($yesterday=false)
	{
		if ($yesterday)
			return 'ACTIVEUSER-' . date('Y-m-d', strtotime('-1 day'));
		else
			return 'ACTIVEUSER-' . date('Y-m-d');
	}

    public static function getWaitingSendEmailKey()
    {
        return 'SENDEMAIL:ALL';
    }
}