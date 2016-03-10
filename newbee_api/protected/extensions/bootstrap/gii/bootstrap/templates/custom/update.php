<?php
/**
 * The following variables are available in this template:
 * - $this: the BootCrudCode object
 */
?>
<?php
echo "<?php\n";
$nameColumn=$this->guessNameColumn($this->tableSchema->columns);
$label=$this->pluralize($this->class2name($this->modelClass));
echo "\$this->breadcrumbs=array(
	'$label'=>array('index'),
	\$model->{$nameColumn}=>array('view','id'=>\$model->{$this->tableSchema->primaryKey}),
	'Update',
);\n";
?>

$this->menu=array(
	array('label'=>'列表 <?php echo $this->modelClass; ?>','url'=>array('index')),
	array('label'=>'添加 <?php echo $this->modelClass; ?>','url'=>array('create')),
	array('label'=>'详情 <?php echo $this->modelClass; ?>','url'=>array('view','id'=>$model-><?php echo $this->tableSchema->primaryKey; ?>)),
);
?>

<?php echo "<?php echo \$this->renderPartial('_form',array('model'=>\$model)); ?>"; ?>