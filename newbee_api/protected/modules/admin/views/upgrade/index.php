<?php
$this->breadcrumbs=array(
	'在线升级'=>array('index'),
	'在线升级',
);

?>

<div id="cleaner-status-grid" class="grid-view">
	<div class="breadcrumb">第 1-10 条, 共 19 条.</div>
	<table class="items table table-striped table-bordered">
		<thead>
			<tr>
				<th id="cleaner-status-grid_c0">净化器ID</th>
				<th id="cleaner-status-grid_c2">升级步骤</th>
			</tr>
		</thead>
		<tbody>
		<?php 
			if (!empty($data))
			 foreach ($data as $cleaner_id => $score)
			{
				echo '<tr>';
				echo '<td>' . $cleaner_id . '</td>';
				echo '<td>' . $score . '</td>';
				echo '</tr>';
			}
		?>
		</tbody>
	</table>
</div>
