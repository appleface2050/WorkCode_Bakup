<?php

/**
 * This is the model class for table "upgrade_program_history".
 *
 * The followings are the available columns in table 'upgrade_program_history':
 * @property string $id
 * @property string $version
 * @property string $client_id
 * @property string $filepath
 * @property string $date_char
 * @property string $add_time
 */
class UpgradeProgram extends CActiveRecord
{
	
	
	 /**
	  * 目录前缀
	  * @var unknown
	  */
	 const DIR_PREFIX = 'version';
	
	/**
	 * @return string the associated database table name
	 */
	public function tableName()
	{
		return 'upgrade_program_history';
	}

	/**
	 * @return array validation rules for model attributes.
	 */
	public function rules()
	{
		// NOTE: you should only define rules for those attributes that
		// will receive user inputs.
		return array(
			array('version, client_id, filepath, date_char, add_time', 'required'),
			// The following rule is used by search().
			// @todo Please remove those attributes that should not be searched.
			array('id, version, client_id, filepath, date_char, add_time, description', 'safe'),
		);
	}

	/**
	 * @return array relational rules.
	 */
	public function relations()
	{
		// NOTE: you may need to adjust the relation name and the related
		// class name for the relations automatically generated below.
		return array(
		);
	}

	/**
	 * 在验证之前(non-PHPdoc)
	 * @see CModel::beforeValidate()
	 */
	public function beforeValidate()
	{
		if($this->getIsNewRecord())
		{
			$file = CUploadedFile::getInstanceByName('filepath');
			if(!empty($file))
			{
				$this->filepath = $this->getDir() . '/' . $this->getFilename();
				$file->saveAs(Yii::getPathOfAlias('webroot') . '/' . self::DIR_PREFIX . '/' . $this->filepath);
			}
		}
		return parent::beforeValidate();
	}
	
	/**
	 * @return array customized attribute labels (name=>label)
	 */
	public function attributeLabels()
	{
		return array(
			'id' => 'ID',
			'version' => 'Version',
			'client_id' => 'Client',
			'filepath' => 'Filepath',
			'date_char' => 'Date Char',
			'add_time' => 'Add Time',
		);
	}

	/**
	 * Retrieves a list of models based on the current search/filter conditions.
	 *
	 * Typical usecase:
	 * - Initialize the model fields with values from filter form.
	 * - Execute this method to get CActiveDataProvider instance which will filter
	 * models according to data in model fields.
	 * - Pass data provider to CGridView, CListView or any similar widget.
	 *
	 * @return CActiveDataProvider the data provider that can return the models
	 * based on the search/filter conditions.
	 */
	public function search()
	{
		// @todo Please modify the following code to remove attributes that should not be searched.

		$criteria=new CDbCriteria;

		$criteria->compare('id',$this->id,true);
		$criteria->compare('version',$this->version,true);
		$criteria->compare('client_id',$this->client_id,true);
		$criteria->compare('filepath',$this->filepath,true);
		$criteria->compare('date_char',$this->date_char,true);
		$criteria->compare('add_time',$this->add_time,true);

		return new CActiveDataProvider($this, array(
			'criteria'=>$criteria,
		));
	}

	/**
	 * Returns the static model of the specified AR class.
	 * Please note that you should have this exact method in all your CActiveRecord descendants!
	 * @param string $className active record class name.
	 * @return UpgradeProgram the static model class
	 */
	public static function model($className=__CLASS__)
	{
		return parent::model($className);
	}
	
	/**
	 * 升级文件保存路径
	 */
	public function getDir()
	{
		$date = date('Ym');
		$webroot = Yii::getPathOfAlias('webroot');
		$dir = $webroot . '/' . self::DIR_PREFIX . '/' . $date . '/';
		if (!is_dir($dir))
		{
			mkdir($dir);
			chmod($dir, 0777);			
		}
		return $date;
	}
	
	/**
	 * 获取文件名
	 */
	public function getFilename()
	{
		return date('His') . rand(10,99);
	}
	
	/**
	 * 获取更新文件的绝对路径
	 * @param unknown $path
	 */
	public function getAbsoluteFilepath($path)
	{
		return Yii::getPathOfAlias('webroot') . '/' . self::DIR_PREFIX . '/' . $path;
	}
	
	/**
	 * 获取最新的版本
	 * @param unknown $type
	 */
	public function getLatestVersion($client_id)
	{
		$sql = "SELECT `version` FROM " . $this->tableName() . " WHERE `client_id`='" . $client_id . "' ORDER BY id DESC LIMIT 1";
		$version = Yii::app()->db->createCommand($sql)->queryScalar();
		return !empty($version) ? $version : '';
	}
}
