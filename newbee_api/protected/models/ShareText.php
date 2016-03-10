<?php

/**
 * This is the model class for table "share_text".
 *
 * The followings are the available columns in table 'share_text':
 * @property string $id
 * @property string $title
 * @property string $content
 * @property string $page_url
 * @property integer $create_time
 */
class ShareText extends CActiveRecord
{
	/**
	 * @return string the associated database table name
	 */
	public function tableName()
	{
		return 'share_text';
	}

	/**
	 * @return array validation rules for model attributes.
	 */
	public function rules()
	{
		// NOTE: you should only define rules for those attributes that
		// will receive user inputs.
		return array(
			array('create_time', 'default', 'value'=>time()),
			array('title, content, page_url, create_time', 'required'),
            array('image_path',
                'file',    //定义为file类型
                'allowEmpty'=>true,
                'types'=>'jpg,jpeg,gif,png',   //上传文件的类型
                'maxSize'=>1024 * 300,    //上传大小限制，注意不是php.ini中的上传文件大小
                'tooLarge'=>'请上传小于300k的文件！'
            ),
            array('title, page_url', 'length', 'max'=>100),
            array('page_url', 'url','message'=>'必须是一个完整的url地址如：http://www.baidu.com'),
			array('content', 'length', 'max'=>200),
			// The following rule is used by search().
			// @todo Please remove those attributes that should not be searched.
			array('id, title, content, page_url, create_time', 'safe', 'on'=>'search'),
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
	 * @return array customized attribute labels (name=>label)
	 */
	public function attributeLabels()
	{
		return array(
			'id' => 'ID',
			'title' => '标题',
			'content' => '内容',
			'page_url' => '页面url',
            'image_path' => '图片',
			'create_time' => '创建时间',
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
		$criteria->compare('title',$this->title,true);
		$criteria->compare('content',$this->content,true);
		$criteria->compare('page_url',$this->page_url,true);
		$criteria->compare('create_time',$this->create_time);

		return new CActiveDataProvider($this, array(
			'criteria'=>$criteria,
		));
	}

	/**
	 * Returns the static model of the specified AR class.
	 * Please note that you should have this exact method in all your CActiveRecord descendants!
	 * @param string $className active record class name.
	 * @return ShareText the static model class
	 */
	public static function model($className=__CLASS__)
	{
		return parent::model($className);
	}
}
