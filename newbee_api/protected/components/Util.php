<?php

class Util
{
	
	/**
	 * 检测是否是手机号
	 * @param unknown $mobile
	 */
	public static function checkIsMobile($mobile)
	{
		return preg_match('/^(13|14|15|17|18|19)\d{9}$/', $mobile) ? true : false;
	}
	
	/**
	 * 检测是否邮箱
	 * @param unknown $email
	 */
	public static function checkIsEmail($email)
	{
		return preg_match('/^[a-zA-Z0-9!#$%&\'*+\\/=?^_`{|}~-]+(?:\.[a-zA-Z0-9!#$%&\'*+\\/=?^_`{|}~-]+)*@(?:[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?\.)+[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?$/', $email) ? true : false;
	}
	
	/**
	 * 检测是否是验证码
	 * @param unknown $code
	 * @param number $length
	 */
	public static function checkVerifyCode($code, $length=4)
	{
		$pattern = "/^\d{".$length."}$/";
		return preg_match($pattern, $code) ? true : false;
	}
	
	/**
	 * 获取验证码
	 * @param int $length 长度,默认是4
	 */
	public static function getVerifyCode($length=4)
	{
		$code = '';
		for ($i=0; $i<$length; $i++)
			$code .= rand(0, 9);
		return $code;
	}
	
	/**
	 * 检测时间点格式,如 22:30
	 * @param unknown $time_point
	 */
	public static function checkTimePoint($time_point)
	{
		return preg_match('/^\d{2}\:\d{2}$/', $time_point) ? true : false;
	}
	/**
	 * 获取首字母
	 */
	public static function getFirstLetter($str)
	{
		$s0 = mb_substr($str, 0,1,'UTF-8');
	
		$exclude = array(
				'逍'	=> 'X','郜' => 'G','裘' => 'Q','禚' => 'Z',
				'闫' => 'Y','蔺' => 'L','缪' => 'M','栾' => 'L',
				'邬' => 'W', '覃' => 'Q', '臧' => 'Z','谌' => 'C',
				'郏' => 'J','佟' => 'T','褚' => 'C','佘' => 'S',
				'岑' => 'C','闵' => 'M','莘' => 'S','晏' => 'Y','椁' => 'G',
				'瞿' => 'Q','胥' => 'X','郇' => 'X','玺' => 'X','阙' => 'Q',
				'滕' => 'T','笪' => 'D','昃' => 'Z','冼' => 'X','鄢' => 'Y'
		);
	
		if(key_exists($s0, $exclude))
		{
			return $exclude[$s0];
		}
		$fchar = ord($s0{0});
		if($fchar >= ord("1") and $fchar <= ord("z") )return strtoupper($s0{0});
		$s1 = iconv("UTF-8","GB2312", $s0);
		$s2 = iconv("GB2312","UTF-8", $s1);
		if($s2 == $s0){$s = $s1;}else{$s = $s0;}
		$asc = ord($s{0}) * 256 + ord($s{1}) - 65536;
		if($asc >= -20319 and $asc <= -20284) return "A";
		if($asc >= -20283 and $asc <= -19776) return "B";
		if($asc >= -19775 and $asc <= -19219) return "C";
		if($asc >= -19218 and $asc <= -18711) return "D";
		if($asc >= -18710 and $asc <= -18527) return "E";
		if($asc >= -18526 and $asc <= -18240) return "F";
		if($asc >= -18239 and $asc <= -17923) return "G";
		if($asc >= -17922 and $asc <= -17418) return "H";
		if($asc >= -17417 and $asc <= -16475) return "J";
		if($asc >= -16474 and $asc <= -16213) return "K";
		if($asc >= -16212 and $asc <= -15641) return "L";
		if($asc >= -15640 and $asc <= -15166) return "M";
		if($asc >= -15165 and $asc <= -14923) return "N";
		if($asc >= -14922 and $asc <= -14915) return "O";
		if($asc >= -14914 and $asc <= -14631) return "P";
		if($asc >= -14630 and $asc <= -14150) return "Q";
		if($asc >= -14149 and $asc <= -14091) return "R";
		if($asc >= -14090 and $asc <= -13319) return "S";
		if($asc >= -13318 and $asc <= -12839) return "T";
		if($asc >= -12838 and $asc <= -12557) return "W";
		if($asc >= -12556 and $asc <= -11848) return "X";
		if($asc >= -11847 and $asc <= -11056) return "Y";
		if($asc >= -11055 and $asc <= -10247) return "Z";
		return '';
	}

    /**
     * 加密解密方法
     * @param $string
     * @param string $operation
     * @param string $key
     * @param int $expiry
     * @return string
     */
	public static function encrypt($string, $operation = 'DECODE', $key = '', $expiry = 0)
	{
		$ckey_length = 4;
		$key = md5($key != '' ? $key : 'sangebaba@#￥%&*cleaner');
		$keya = md5(substr($key, 0, 16));
		$keyb = md5(substr($key, 16, 16));
		$keyc = $ckey_length ? ($operation == 'DECODE' ? substr($string, 0, $ckey_length): substr(md5(microtime()), -$ckey_length)) : '';

		$cryptkey = $keya.md5($keya.$keyc);
		$key_length = strlen($cryptkey);

		$string = $operation == 'DECODE' ? base64_decode(substr($string, $ckey_length)) : sprintf('%010d', $expiry ? $expiry + time() : 0).substr(md5($string.$keyb), 0, 16).$string;
		$string_length = strlen($string);

		$result = '';
		$box = range(0, 255);

		$rndkey = array();
		for($i = 0; $i <= 255; $i++) {
			$rndkey[$i] = ord($cryptkey[$i % $key_length]);
		}

		for($j = $i = 0; $i < 256; $i++) {
			$j = ($j + $box[$i] + $rndkey[$i]) % 256;
			$tmp = $box[$i];
			$box[$i] = $box[$j];
			$box[$j] = $tmp;
		}

		for($a = $j = $i = 0; $i < $string_length; $i++) {
			$a = ($a + 1) % 256;
			$j = ($j + $box[$a]) % 256;
			$tmp = $box[$a];
			$box[$a] = $box[$j];
			$box[$j] = $tmp;
			$result .= chr(ord($string[$i]) ^ ($box[($box[$a] + $box[$j]) % 256]));
		}

		if($operation == 'DECODE') {
			if((substr($result, 0, 10) == 0 || substr($result, 0, 10) - time() > 0) && substr($result, 10, 16) == substr(md5(substr($result, 26).$keyb), 0, 16)) {
				return substr($result, 26);
			} else {
				return '';
			}
		} else {
			return $keyc.str_replace('=', '', base64_encode($result));
		}
	}
}