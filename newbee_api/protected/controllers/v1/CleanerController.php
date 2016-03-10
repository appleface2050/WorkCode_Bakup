<?php

/**
 * 净化器管理
 * @author zhoujianjun
 *
 */
class CleanerController extends RestController
{
	
	/**
	 * 净化器首页  返回全部净化器
	 */
	public function actionIndex()
	{
		$result = UserCleaner::model()->early()->with(array('cleaner'=>array('with'=>'operator')))->findAllByAttributes(array('user_id'=>Yii::app()->user->id,'is_verify'=>UserCleaner::IS_VERIFY_YES));
		$data = array();
		if (!empty($result))
		{
			foreach ($result as $value)
			{
				$cleaner = $value->cleaner;
				if (!empty($cleaner))
				{
					// 最近操作的人
					$operator = $cleaner->operator;
					$tmp = array(
						'id' => $cleaner->id,
						'status' => intval($cleaner->status),
						'type' => intval($cleaner->type),
						'name' => $value->name,
                        'city' => $cleaner->city,
						'level' => intval($cleaner->level),
						'childlock' => intval($cleaner->childlock),
						'automatic' => intval($cleaner->automatic),
						'pm_outer_index' => Yii::app()->location->getPm($cleaner->city),
						'aqi' => intval($cleaner->aqi),
						'timeSet' => empty($cleaner->timeset) ? array() : json_decode($cleaner->timeset,true),
						'is_open' => intval($cleaner->switch),
						'tip' => CleanerStatus::model()->getFilterTip(json_decode($cleaner->filter_surplus_life, true), intval($cleaner->type)),
						'voc' => intval($cleaner->voc),
						'in_timeset' => 0
						//'in_timeset' => CleanerStatus::model()->checkInSilence($cleaner)  静音时间设置已经去掉
					);
					// 最后操作的人
					//$tmp['last_operator'] = (!empty($operator) && $cleaner->operator_uid != Yii::app()->user->id) ? $operator->nickname : '';
					$tmp['last_operator'] = '';
					$data[$cleaner->id] = $tmp;
				}
			}
		}
		
		$this->success($data);
	}
	
	
	
	/**
	 * 添加净化器 (在app端添加净化器之前,净化器硬件端已经初始化完毕)
	 * @method POST
	 * @param string $token 
	 * @param string $wifi_name
	 * @param string $wifi_pwd
	 * @param string $id  净化器id
	 * @param string $name 净化器命名
	 * @param string $point_x 经度
	 * @param string $point_y 纬度
	 * @param string $sign
	 * 
	 * @todo mac 入网的路由器的硬件地址
	 * 
	 */
	public function actionCreate()
	{
		Yii::log(print_r($_REQUEST, true));
		
		$request = Yii::app()->request;
		$id = strtoupper($request->getParam('id'));
		$qrcode = $request->getParam('qrcode');
		$cleaner = CleanerStatus::model()->findByPk($id);
		if (empty($cleaner))
		{
			// 首次添加
			$cleaner = new CleanerStatus();
			$cleaner->filter_surplus_life = json_encode(CleanerStatus::getDefaultLife(CleanerStatus::TYPE_BIG_WITH));
		}
		
		$point_x = $request->getParam('point_x');  //经度
		$point_y = $request->getParam('point_y');  //纬度
		$city = Yii::app()->location->getCity($point_x, $point_y);  //获取经纬度所在的城市
		
		$cleaner->attributes = array(
			'id' => $id,
			'qrcode' => $qrcode,
			'point_x' => $point_x,
			'point_y' => $point_y,
			'city' => $city
		);
		
		if ($cleaner->save())
		{
			// 如果已经添加了,则更新相关信息
			$model = UserCleaner::model()->findByAttributes(array('user_id' => Yii::app()->user->id, 'cleaner_id' => $id));
			if (empty($model))
			{
				$model = new UserCleaner();
			}
			$model->attributes = array(
				'user_id' => Yii::app()->user->id,
				'cleaner_id' => $id,
				'name' => trim($request->getParam('name')),
				'wifi_name' => trim($request->getParam('wifi_name')),
				'wifi_pwd' => trim($request->getParam('wifi_pwd')),
				'point_x' => $point_x,
				'point_y' => $point_y,
				'city' => $city,
				'add_time' => time()
			);
			if (!$model->save())
				$this->failed('添加失败');
		} else 
		{
			$error = array_values($cleaner->getErrors());
			$this->failed($error[0][0]);
		}
		$this->success();
	}
	
	/**
	 * 查看用户的净化器(净化器的名称查看包含的芯片的剩余寿命)
	 * @method GET
	 * @param string $token
	 * @param string $id 净化器id
	 */
	public function actionView()
	{
		$model = $this->loadModel(Yii::app()->user->id, Yii::app()->request->getParam('id'), true);
		$cleaner = $model->cleaner;
		
		$update = CleanerStatus::model()->checkUpgrade($cleaner);
		$data = array(
			'id' => $model->cleaner_id,
			'qrcode' => $cleaner->qrcode,
			'name' => $model->name,
			'version' => !empty($cleaner->version) ? $cleaner->version : CleanerStatus::VERSION_DEFAULT
		);
		if ($update)
		{
			$data['latestVersion'] = $update;
			$data['update'] = 1;
		}
		else 
		{
			$data['update'] = 0;
		}
		
		// 滤芯的默认数据
        /*$surplus_life = json_decode($cleaner->filter_surplus_life, true);
        $surplus = array();
        $type = $cleaner->type;
        $i=1;
        foreach ($surplus_life as $id => $life)
        {
            $defaultLife = intval(CleanerStatus::getDefaultLife($type, $i));
            $per = number_format(intval($life)/200000, 2) * 100;
            $per = $per > 100 ? '100%' : $per . '%';
            $surplus[] = array('id' => $id, 'name' => CleanerStatus::getDefaultName($type, $i), 'surplus_life' => $per);
            $i++;
        }*/

        $surplus = CleanerStatus::countSurplusLife($cleaner->filter_surplus_life,$cleaner->type,$cleaner->version);
		$data['filter'] = $surplus;
		$this->success($data);
	}
	
	/**
	 * 更改净化器的名字
	 * @method POST
	 * @param string $token
	 * @param string $id 净化器id
	 * @param string $name 更改后的名字
	 * @param string $sign 签名字符串
	 */
	public function actionUpdate()
	{
		$model = $this->loadModel(Yii::app()->user->id, Yii::app()->request->getParam('id'));
		$model->updateByPk($model->id, array('name' => trim(Yii::app()->request->getParam('name'))));
		$this->success();
	}
	
	/**
	 * 删除净化器
	 * @method POST
	 * @param string $token
	 * @param string $id 净化器id
	 * @param string $sign 签名字符串
	 */
	public function actionDelete()
	{
		$model = $this->loadModel(Yii::app()->user->id, Yii::app()->request->getParam('id'));
		$model->deleteByPk($model->id);
		$this->success();
	}
	
	/**
	 * 净化器运行参数(室内外pm25值 和 各运行参数)
	 * @method GET
	 * @param string $token
	 * @param string $id 净化器id
	 */
	public function actionParameter()
	{
		$data = CleanerStatus::model()->detail(Yii::app()->request->getParam('id'));
		$this->success($data);
	}
	
	
	/**
	 * 检测某一净化器序列号是否已经添加
	 * @param string token
	 * @param string $id 净化器id
	 * @deprecated  弃用了
	 */
	public function actionExist()
	{
		$qrcode = Yii::app()->request->getParam('qrcode');
		$cleaner = CleanerStatus::model()->findByAttributes(array('qrcode' => $qrcode));
		$model = UserCleaner::model()->findByAttributes(array('qrcode' => $qrcode, 'user_id'=>Yii::app()->user->id));
		$data = empty($model) ? array('exist' => 0) : array('exist' => 1);
		$this->success($data);
	}
	
	/**
	 * 净化器添加简化版
	 * @param string qrcode
	 * @param string name
	 */
	public function actionSimple()
	{
		$qrcode = Yii::app()->request->getParam('qrcode');
		if (!Yii::app()->user->is_admin)
		{
			// 普通账户,判断序列号是否存在
			$cleanerQR = Cleaner::model()->findByAttributes(array('qrcode' => $qrcode));
			if (empty($cleanerQR))
				throw new CHttpException(400, '序列号不存在');
		}
		$name = trim(Yii::app()->request->getParam('name'));
		$cleaner = CleanerStatus::model()->findByAttributes(array('qrcode' => $qrcode));
		if (empty($cleaner))
			throw new CHttpException(404, '序列号错误');
		if (!$this->checkOnline($cleaner->id))
			throw new CHttpException(400, '净化器不在线');
		
		// 绑定用户和净化器的关系,如果已经添加了,则更新相关信息
		$model = UserCleaner::model()->findByAttributes(array('user_id' => Yii::app()->user->id, 'cleaner_id' => $cleaner->id));
		if (empty($model))
		{
			$model = new UserCleaner();
		}
		$model->attributes = array(
			'user_id' => Yii::app()->user->id,
			'cleaner_id' => $cleaner->id,
			'name' => $name,
			'add_time' => time()
		);
		$model->is_verify = UserCleaner::IS_VERIFY_YES;
		$model->is_charger = UserCleaner::IS_CHARGER_YES;
		if (!$model->save())
		{
			$error = array_values($model->getErrors());
			$this->failed($error[0][0]);
		}
		$this->success();
	}
	
	/**
	 * @param string qrcode
	 * 检测净化器状态
	 * @todo 检测序列号是否合法
	 */
	public function actionCheck()
	{
		$qrcode = Yii::app()->request->getParam('qrcode');
		if (!Yii::app()->user->is_admin)
		{
			$cleanerQR = Cleaner::model()->findByAttributes(array('qrcode' => $qrcode));
			if (empty($cleanerQR))
				throw new CHttpException(400, '序列号不存在');
		}
		$cleaner = CleanerStatus::model()->findByAttributes(array('qrcode' => $qrcode));
		if (empty($cleaner))
			$this->success(array('simple' => 0));  // 未添加过完整的流程
		$model = UserCleaner::model()->findByAttributes(array('cleaner_id' => $cleaner->id, 'user_id'=>Yii::app()->user->id));
		if (!empty($model))
			$this->failed('已经添加过了');
		if ($this->checkOnline($cleaner->id))
			$this->success(array('simple' =>1));   // 净化器在线,走简化流程
		else
			$this->success(array('simple' =>0));   // 净化器不在线,走完整流程
	}
	
	/**
	 * 升级状态(检测升级是否完成, 完成返回1 否则返回0)
	 */
	public function actionUpgradeStatus()
	{
		$model = $this->loadModel(Yii::app()->user->id, Yii::app()->request->getParam('id'), true);
		$cleaner = $model->cleaner;
		
		// 升级状态
		$finish = 0;
		
		if (!Yii::app()->redis->sIsMember(EKeys::getUpgradeKey(), $cleaner->id))
			$finish = 1;   // 升级完成

		$this->success(array('finish' => $finish));
		
	}
	
	/**
	 * 净化器切换城市
	 */
	public function actionLocation()
	{
		$model = $this->loadModel(Yii::app()->user->id, Yii::app()->request->getParam('id'), true);
		$city = trim(Yii::app()->request->getParam('city'));
		if (empty($city))
			throw new CHttpException(400, '缺少参数');
		$model->cleaner->updateByPk($model->cleaner_id, array('city' => $city));
		$this->success();
	}
	
	/**
	 * 获取用户的某个净化器
	 * @param unknown $cleaner_id 
	 * @param unknown $user_id
	 * @param $withCleaner 是否获取净化器的信息,默认不获取
	 */
	public function loadModel($user_id, $cleaner_id, $withCleaner=false)
	{
		if ($withCleaner)
			$model = UserCleaner::model()->with('cleaner')->findByAttributes(array('cleaner_id' => $cleaner_id, 'user_id'=>$user_id));
		else
			$model = UserCleaner::model()->findByAttributes(array('cleaner_id' => $cleaner_id, 'user_id'=>$user_id));
		if (empty($model))
			throw new CHttpException(404, '净化器不存在');
		return $model;
	}
	
}