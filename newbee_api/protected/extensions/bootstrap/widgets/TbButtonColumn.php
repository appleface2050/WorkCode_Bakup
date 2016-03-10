<?php
/**
 * TbButtonColumn class file.
 * @author Christoffer Niska <ChristofferNiska@gmail.com>
 * @copyright  Copyright &copy; Christoffer Niska 2011-
 * @license http://www.opensource.org/licenses/bsd-license.php New BSD License
 * @package bootstrap.widgets
 * @since 0.9.8
 */

Yii::import('zii.widgets.grid.CButtonColumn');

/**
 * Bootstrap button column widget.
 * Used to set buttons to use Glyphicons instead of the defaults images.
 */
class TbButtonColumn extends CButtonColumn
{
	/**
	 * @var array the HTML options for the data cell tags.
	 */
	public $htmlOptions=array('class'=>'');
	/**
	 * @var array the HTML options for the header cell tag.
	*/
	public $headerHtmlOptions=array('class'=>'');
	/**
	 * @var array the HTML options for the footer cell tag.
	*/
	public $footerHtmlOptions=array('class'=>'');
	
	/**
	 * @var string the view button icon (defaults to 'eye-open').
	 */
	public $viewButtonIcon = 'icon-zoom-in icon-white';
	/**
	 * @var string the update button icon (defaults to 'pencil').
	 */
	public $updateButtonIcon = 'icon-edit icon-white';
	/**
	 * @var string the delete button icon (defaults to 'trash').
	 */
	public $deleteButtonIcon = 'icon-trash icon-white';

	/**
	 * @var array the HTML options for the view button tag.
	 */
	public $viewButtonOptions=array('class'=>'btn btn-mini btn-success');
	
	/**
	 * @var array the HTML options for the update button tag.
	 */
	public $updateButtonOptions=array('class'=>'btn btn-mini btn-info');
	
	/**
	 * @var array the HTML options for the delete button tag.
	 */
	public $deleteButtonOptions=array('class'=>'btn btn-mini btn-danger');
	
	/**
	 * @var string the label for the view button. Defaults to "View".
	 * Note that the label will not be HTML-encoded when rendering.
	 */
	public $viewButtonLabel = '查看';
	
	/**
	 * Initializes the default buttons (view, update and delete).
	 */
	protected function initDefaultButtons()
	{
		parent::initDefaultButtons();

		if ($this->viewButtonIcon !== false && !isset($this->buttons['view']['icon']))
			$this->buttons['view']['icon'] = $this->viewButtonIcon;
		if ($this->updateButtonIcon !== false && !isset($this->buttons['update']['icon']))
			$this->buttons['update']['icon'] = $this->updateButtonIcon;
		if ($this->deleteButtonIcon !== false && !isset($this->buttons['delete']['icon']))
			$this->buttons['delete']['icon'] = $this->deleteButtonIcon;
	}

	/**
	 * Renders a link button.
	 * @param string $id the ID of the button
	 * @param array $button the button configuration which may contain 'label', 'url', 'imageUrl' and 'options' elements.
	 * @param integer $row the row number (zero-based)
	 * @param mixed $data the data object associated with the row
	 */
	protected function renderButton($id, $button, $row, $data)
	{
		if (isset($button['visible']) && !$this->evaluateExpression($button['visible'], array('row'=>$row, 'data'=>$data)))
			return;

		$label = isset($button['label']) ? $button['label'] : $id;
		$url = isset($button['url']) ? $this->evaluateExpression($button['url'], array('data'=>$data, 'row'=>$row)) : '#';
		$options = isset($button['options']) ? $button['options'] : array();

		if (!isset($options['title']))
			$options['title'] = $label;

// 		if (!isset($options['rel']))
// 			$options['rel'] = 'tooltip';

		if (isset($button['icon']))
		{
			if (strpos($button['icon'], 'icon') === false)
				$button['icon'] = 'icon-'.implode(' icon-', explode(' ', $button['icon']));

			echo CHtml::link('<i class="'.$button['icon'].'"></i>'."&nbsp;". $label, $url, $options);
		}
		else if (isset($button['imageUrl']) && is_string($button['imageUrl']))
			echo CHtml::link(CHtml::image($button['imageUrl'], $label), $url, $options);
		else
			echo CHtml::link($label, $url, $options);
	}
}
