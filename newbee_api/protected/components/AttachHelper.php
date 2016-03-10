<?php

/**
 * 
 *
 *
 */
class AttachHelper extends CComponent
{

	const BASEROOT = 'attach/';

    private $root;     //存数图片的跟路径

    public $file;      //文件对象

    public $extension; //扩展名

    public $filename;  //新文件名

    public $dir;

	public function __construct($file)
	{
		// 客户端通过FILE上传
		if($file instanceof CUploadedFile)
            $this->file = $file;

        /*$filesize=$file->getSize();//获取文件大小
        $filetype=$file->getType();//获取文件类型*/

        $this->root = Yii::getPathOfAlias('webroot') . '/' . self::BASEROOT;
        $this->getExtensionName();
        $this->getFilename();
        $this->getDirname();

	}
	
	/**
	 * 获取文件名
	 */
	public function getFilename()
	{
        $this->filename = date("Hi").'_'.substr(md5(time() . '_' . mt_rand(1,99999)),0,8);
	}

	/**
	 * 获取扩展名
	 */
	public function getExtensionName()
	{
        $this->extension = $this->file->getExtensionName();
	}
	
	/**
	 * 获取目录
	 */
	public function getDirname()
	{
        $tmp = explode('/', date("Ym/d"));
        $this->dir = $tmp[0] . '/' . $tmp[1] . '/';
        self::mkdir($this->root . $this->dir, true);
	}
	
	/**
	 * 创建目录
	 */
	public static function mkdir($dst, $recursive=true)
	{
		if(!is_dir($dst))
		{
			$prevDir=dirname($dst);
			if($recursive && !is_dir($dst) && !is_dir($prevDir))
				self::mkdir($prevDir,true);
	
			$res=mkdir($dst, 0777);
			chmod($dst,0777);
			return $res;
		}
	}
	
	
	/**
	 * 保存原始数据
	 * @param unknown $savePath
	 */
	private function doSave($savePath)
	{
		// 客户端上传
		$this->file->saveAs($savePath);
	}
	
	/**
	 * 保存图片, 并返回保存后的地址
	 */
	public function save()
	{
		$savePath = $this->root .  $this->dir . $this->filename . '.' . $this->extension;
		$this->doSave($savePath);
		return $this->dir . $this->filename . '.' . $this->extension;
	}
	
	
	/**
	 * 获取图片的地址 
	 * @param unknown $path
	 * @param unknown $type
	 * @return string
	 */
	public static function getFullpath($path,$width = null,$height = null)
	{
		return self::BASEROOT . $path;
	}

    /**
     * 获取图片完整的url
     * @param $path
     * @param null $width
     * @param null $height
     * @return string
     */
    public static function getFileUrl($path,$width = null,$height = null)
    {
		if(!$path)
			return '';
        return Yii::app()->request->hostInfo .'/'.self::getFullpath($path);
    }
}