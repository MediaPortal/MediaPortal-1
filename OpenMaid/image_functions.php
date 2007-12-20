<?php


function CreateThumbnail($image)
{

	$returnimage = str_replace (".jpg", ".mini.jpg", $image);

	$size = GetImageSize2($image,"original");
	$src_w = $size[0];
	$src_h = $size[1];

	$src_im = ImageCreateFromJpeg($image);

	$size2 = GetImageSize2($image, "mini");
	$dst_w = $size2[0];
	$dst_h = $size2[1];

	$dst_im = ImageCreateTrueColor($dst_w,$dst_h);
	ImageCopyResampled($dst_im,$src_im,0,0,0,0,$dst_w,$dst_h,$src_w,$src_h);

  	createImageBorder($dst_im);

	ImageJpeg($dst_im,$returnimage,90);

	ImageDestroy($dst_im);
	imageDestroy($src_im);
	
	return $returnimage;
}

function GetImageSize2($img,$typeimage)
{
	$img_l_max = 1600;
	$img_h_max = 1200;

	$img_detail_dl_l_max = 133;
	$img_detail_dl_h_max = 100;


	switch($typeimage)
	{
		case "original" : $lmax = $img_l_max; $hmax = $img_h_max; break;
		case "mini" : $lmax = $img_detail_dl_l_max; $hmax = $img_detail_dl_h_max; break;
	}


	if(!empty($img))
	{
		if(is_string(strstr($img,"http://")))
		{
			$size = CalculateSizeImage($img,$lmax,$hmax);
		}
		else if(@file_exists($img))
		{
			$size = CalculateSizeImage($img,$lmax,$hmax);
		}
	}

	if(is_array(@$size))return $size;
	else return false;
}


function CalculateSizeImage($img,$lmax,$hmax)
{
	$OriginalSize = @getimagesize($img);
	if(is_array($OriginalSize))
	{
		$l = $OriginalSize[0];
		$h = $OriginalSize[1];
		if($l>$h) //landscape image
		{
			if($l<$lmax)
			{
				$size[0]=$l;
				$size[1]=$h;
			}
			else
			{
				$size[0]=$lmax;
				$size[1]=intval($h*$lmax/$l);
			}
		}
		else // other type image
		{
			if($h<$hmax)
			{
				$size[0]=$l;
				$size[1]=$h;
			}
			else
			{
				$size[0]=intval($l*$hmax/$h);
				$size[1]=$hmax;
			}
		}
	}
	return @$size;
}

function createImageBorder($scr_img)
{
 $width  = imagesx($scr_img);
 $height  = imagesy($scr_img);
 $borderColor = 0;

 // line a - b
 $abX  = 0;
 $abY  = 0;
 $abX1 = $width;
 $abY1 = 0;

 // line a - c
 $acX  = 0;
 $acY  = 0;
 $acX1 = 0;
 $acY1 = $height;

 // line b - d
 $bdX  = $width-1;
 $bdY  = 0;
 $bdX1 = $width-1;
 $bdY1 = $height;

 // line c - d
 $cdX  = 0;
 $cdY  = $height-1;
 $cdX1 = $width;
 $cdY1 = $height-1;

 // DRAW LINES
 imageline($scr_img,$abX,$abY,$abX1,$abY1,$borderColor);
 imageline($scr_img,$acX,$acY,$acX1,$acY1,$borderColor);
 imageline($scr_img,$bdX,$bdY,$bdX1,$bdY1,$borderColor);
 imageline($scr_img,$cdX,$cdY,$cdX1,$cdY1,$borderColor);

}


?>
