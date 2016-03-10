<?php
/**
 * Curl wrapper for Yii
 */
class ECurl extends CApplicationComponent {
	/**
	 * curl实例句柄
	 */
	private $_ch;
	
	/**
	 * curl配置 
	 */
	public $options;
	
	/**
	 * 默认的curl配置 
	 */
	private $_config = array(
		CURLOPT_RETURNTRANSFER => true,
		CURLOPT_FOLLOWLOCATION => true,
		CURLOPT_AUTOREFERER => true,
		CURLOPT_CONNECTTIMEOUT => 60,
		CURLOPT_TIMEOUT => 60,
		CURLOPT_SSL_VERIFYPEER => false,
		CURLOPT_USERAGENT => 'Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:5.0) Gecko/20110619 Firefox/5.0'
	);

	/**
	 * 发送请求
	 * @param string $url
	 * @return string
	 */
	private function _exec($url) {
		$this->setOption(CURLOPT_URL, $url);
		$c = curl_exec($this->_ch);
		if (!curl_errno($this->_ch)) {
			return $c; 
		} else {
			throw new CException(curl_error($this->_ch));
		}
	}

	/**
	 * get method 
	 * @param string $url
	 * @param array $data
	 * @return string
	 */
	public function get($url, $params = array()) {
		$this->setOption(CURLOPT_HTTPGET, true);
		return $this->_exec($this->buildUrl($url, $params));
	}

	/**
	 * post method 
	 * @param string $url
	 * @param array $data
	 * @return string
	 */
	public function post($url, $data) {
		$this->setOption(CURLOPT_HTTPHEADER, array("Expect:"));
		$this->setOption(CURLOPT_POST, true);
		$this->setOption(CURLOPT_POSTFIELDS, $data);
		$this->setOption(CURLOPT_SSL_VERIFYPEER, 0);
		$this->setOption(CURLOPT_SSL_VERIFYHOST, FALSE);
		return $this->_exec($url);
	}

	/**
	 * put method
	 * @param string $url
	 * @param mixed $data
	 * @param array $params
	 * @return string
	 */
	public function put($url, $data, $params = array()) {
		// write to memory/temp
		$f = fopen('php://temp', 'rw+');
		fwrite($f, $data);
		rewind($f);

		$this->setOption(CURLOPT_PUT, true);
		$this->setOption(CURLOPT_INFILE, $f);
		$this->setOption(CURLOPT_INFILESIZE, strlen($data));
		return $this->_exec($this->buildUrl($url, $params));
	}

	/**
	 * 构造请求url
	 * 
	 * @param string $url
	 * @param array $data
	 */
	public function buildUrl($url, $data = array()) {
		$parsed = parse_url($url);
		isset($parsed['query']) ? parse_str($parsed['query'], $parsed['query']) : $parsed['query'] = array();
		$params = isset($parsed['query']) ? array_merge($parsed['query'], $data) : $data;
		$parsed['query'] = ($params) ? '?'.http_build_query($params, '', '&') : '';
		if (!isset($parsed['path'])) {
			$parsed['path'] = '/';			
		}

		$port = isset($parsed['port']) ? ':'.$parsed['port'] : '';
		Yii::trace($parsed['scheme'].'://'.$parsed['host'].$port.$parsed['path'].$parsed['query']);
		return $parsed['scheme'].'://'.$parsed['host'].$port.$parsed['path'].$parsed['query'];
	}

	public function getInfo()
	{
		return curl_getinfo ($this->_ch);
	}
	/**
	 * 批量设置curl参数
	 * 
	 * @param array $options
	 * @return $this 
	 */
	public function setOptions($options = array()) {
		curl_setopt_array($this->_ch, $options);
		return $this;
	}

	/**
	 * 设置curl参数
	 * 
	 * @param mixed $option
	 * @param mixed $value
	 * @return $this
	 */
	public function setOption($option, $value) {
		curl_setopt($this->_ch, $option, $value);
		return $this;
	}

	/**
	 * 初始化 curl相关参数
	 */ 
	public function init() {
		try {
			$this->_ch = curl_init();
			$options = is_array($this->options) ? ($this->options + $this->_config) : $this->_config;
			$this->setOptions($options);

			$ch = $this->_ch;
			// close curl on exit
			Yii::app()->attachEventHandler('onEndRequest', array($this, 'closeCurl'));
			// @todo 如果不兼容，需要修改为正常函数
			//Yii::app()->onEndRequest = function() use(&$ch) {
			//	curl_close($ch);
			//};
		} catch (Exception $e) {
			throw new CException('Curl not installed');
		}
	}
	
	/**
	 * 关闭curl
	 */
	public function closeCurl() {
		curl_close($this->_ch);
	}
}