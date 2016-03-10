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
	\$model->{$nameColumn},
);\n";
?>

$this->menu=array(
	array('label'=>'列表 <?php echo $this->modelClass; ?>','url'=>array('index')),
	array('label'=>'添加 <?php echo $this->modelClass; ?>','url'=>array('create')),
	array('label'=>'编辑 <?php echo $this->modelClass; ?>','url'=>array('update','id'=>$model-><?php echo $this->tableSchema->primaryKey; ?>)),
);
?>

<?php echo "<?php"; ?> $this->widget('bootstrap.widgets.TbDetailView',array(
	'data'=>$model,
	'attributes'=>array(
<?php
foreach($this->tableSchema->columns as $column)
	echo "\t\t'".$column->name."',\n";
?>
	),
)); ?>
