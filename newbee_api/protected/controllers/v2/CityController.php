<?php

/**
 * 城市相关
 *
 */
class CityController extends RestController
{
	
	/**
	 * 获取城市列表
	 */
	public function actionList()
	{
        $cityArr = array();
        $result = AqiIndex::model()->findAll();
        foreach ($result as $val)
        {
            $cityArr[] = $val->city;
        }
        $this->success($cityArr);
    }
	

}