#include "HevcNalDecode.h"
//#include "HevcUtils.h"

#include <iostream>
#include <stdexcept>
#include <string>
// Copyright (C) 2016 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

// ========================================================================
// The code in this file is derived from the 'HEVCESBrowser' project,
// a tool for analyzing HEVC(h265) bitstreams authored by 'virinext'.
// See https://github.com/virinext/hevcesbrowser
// and http://www.codeproject.com/Tips/896030/The-Structure-of-HEVC-Video
// Licensed under the GNU General Public License and 
// the Code Project Open License, http://www.codeproject.com/info/cpol10.aspx
// ========================================================================


#include <sstream>

#include <assert.h>

using namespace HEVC;

NALUnitType HevcNalDecode::processNALUnit(const uint8_t *pdata, std::size_t size, hevchdr& h)
{
	//Note: 'emulation_prevention_three_byte' removal is dealt with inside the BitstreamReader
  BitstreamReader bs(pdata, size);

  NALUnitType type = processNALUnitHeader(bs);

  switch(type)
  {
    case NAL_SPS:
    {
      std::shared_ptr<SPS> psps(new SPS);
      processSPS(psps, bs);
      
      //Initialise to normal values
      h.chromaFormat = psps -> chroma_format_idc;
      
      h.width  = psps -> pic_width_in_luma_samples;
      h.height = psps -> pic_height_in_luma_samples;      
      if (h.height == 1088) h.height = 1080;  // Prevent blur lines 

      h.lumaDepth    = psps -> bit_depth_luma_minus8 + 8; // bit_depth_luma_minus8
      h.chromaDepth  = psps -> bit_depth_chroma_minus8 + 8; // bit_depth_chroma_minus8
      
      h.progressive  = (psps->profile_tier_level.general_progressive_source_flag > 0);
      
      h.profile      = psps->profile_tier_level.general_profile_idc;
      h.level        = psps->profile_tier_level.general_level_idc;

      if(psps -> vui_parameters_present_flag)
      {
        if (psps->vui_parameters.aspect_ratio_info_present_flag)
        {
          h.ar = psps->vui_parameters.aspect_ratio_idc;
          if(h.ar == 255) //EXTENDED_SAR
          {
            h.arx = psps->vui_parameters.sar_width;
            h.ary = psps->vui_parameters.sar_height;
          }   
          else  //Look up the aspect ratio from a table
          {
            struct {int x, y;} ar[] = {{0,0},{1,1},{12,11},{10,11},{16,11},{40,33},{24,11},{20,11},{32,11},{80,33},{18,11},{15,11},{64,33},{160,99},{4,3},{3,2},{2,1}};
            if(h.ar > 16)
            {
              // aspect ratio reserved
              h.ar = 0;
              h.arx = 0;
              h.ary = 0;
            }
            else
            {
              // use preset aspect ratio
              h.arx = ar[h.ar].x;
              h.ary = ar[h.ar].y;
            }
          }    
           
          h.arx *= h.width;
          h.ary *= h.height;
      
          uint32_t a = h.arx, b = h.ary;
          while(a) {uint32_t tmp = a; a = b % tmp; b = tmp;}
          if(b) h.arx /= b, h.ary /= b;
       }

        if (psps->vui_parameters.vui_timing_info_present_flag)
        {
          if ((psps->vui_parameters.vui_time_scale > 0) && (psps->vui_parameters.vui_num_units_in_tick > 0))
          {
            h.AvgTimePerFrame = (__int64)((10000000.0 * (double)psps->vui_parameters.vui_num_units_in_tick)/(double)psps->vui_parameters.vui_time_scale);
          }
        }
      }      

      break;
    }

    // case NAL_PPS:
    // {
    // 
    //   std::shared_ptr<PPS> ppps(new PPS);
    //   processPPS(ppps, bs);
    //   break;
    // }

    default: {}
  };

  return type;
}

//Remove 'emulation_prevention_three_byte' characters and copy to new buffer
void HevcNalDecode::Remove3Byte(uint8_t* dst, const uint8_t* src, int length)
{
	int		si=0;
	int		di=0;
	while(si+2<length){
		//remove escapes (very rare 1:2^22)
		if(src[si+2]>3){
			dst[di++]= src[si++];
			dst[di++]= src[si++];
		}
		else if(src[si]==0 && src[si+1]==0){
			if(src[si+2]==3){ //escape
				dst[di++]= 0;
				dst[di++]= 0;
				si+=3;
				continue;
			}
			else //next start code
				return;
		}

		dst[di++]= src[si++];
	}
}


NALUnitType HevcNalDecode::processNALUnitHeader(BitstreamReader &bs)
{
  //forbidden_zero_bit
  bs.getBit();

  NALUnitType type = (NALUnitType)bs.getBits(6);

  //nuh_layer_id
  bs.getBits(6);

  //nuh_temporal_id_plus1
  bs.getBits(3);
  return type;
}

void HevcNalDecode::processSPS(std::shared_ptr<SPS> psps, BitstreamReader &bs)
{
  psps -> sps_video_parameter_set_id = bs.getBits(4);
  psps -> sps_max_sub_layers_minus1 = bs.getBits(3);
  psps -> sps_temporal_id_nesting_flag = bs.getBits(1);
  psps -> profile_tier_level = processProfileTierLevel(psps -> sps_max_sub_layers_minus1, bs);

  psps -> sps_seq_parameter_set_id = bs.getGolombU();
//  psps -> sps_seq_parameter_set_id = 0;
  psps -> chroma_format_idc = bs.getGolombU();

  if(psps -> chroma_format_idc == 3)
    psps -> separate_colour_plane_flag = bs.getBits(1);
  else
    psps -> separate_colour_plane_flag = 0;

  psps -> pic_width_in_luma_samples = bs.getGolombU();
  psps -> pic_height_in_luma_samples = bs.getGolombU();
  psps -> conformance_window_flag = bs.getBits(1);

  if(psps -> conformance_window_flag)
  {
    psps -> conf_win_left_offset = bs.getGolombU();
    psps -> conf_win_right_offset = bs.getGolombU();
    psps -> conf_win_top_offset = bs.getGolombU();
    psps -> conf_win_bottom_offset = bs.getGolombU();
  }

  psps -> bit_depth_luma_minus8 = bs.getGolombU();
  psps -> bit_depth_chroma_minus8 = bs.getGolombU();
  psps -> log2_max_pic_order_cnt_lsb_minus4 = bs.getGolombU();
  psps -> sps_sub_layer_ordering_info_present_flag = bs.getBits(1);

  psps -> sps_max_dec_pic_buffering_minus1.resize(psps -> sps_max_sub_layers_minus1 + 1, 0);
  psps -> sps_max_num_reorder_pics.resize(psps -> sps_max_sub_layers_minus1 + 1, 0);
  psps -> sps_max_latency_increase_plus1.resize(psps -> sps_max_sub_layers_minus1 + 1, 0);

  for(std::size_t i=(psps -> sps_sub_layer_ordering_info_present_flag ? 0 : psps -> sps_max_sub_layers_minus1);
      i<=psps -> sps_max_sub_layers_minus1;
      i++)
  {
    psps -> sps_max_dec_pic_buffering_minus1[i] = bs.getGolombU();
    psps -> sps_max_num_reorder_pics[i] = bs.getGolombU();
    psps -> sps_max_latency_increase_plus1[i] = bs.getGolombU();
  }

  psps -> log2_min_luma_coding_block_size_minus3 = bs.getGolombU();
  psps -> log2_diff_max_min_luma_coding_block_size = bs.getGolombU();
  psps -> log2_min_transform_block_size_minus2 = bs.getGolombU();
  psps -> log2_diff_max_min_transform_block_size = bs.getGolombU();
  psps -> max_transform_hierarchy_depth_inter = bs.getGolombU();
  psps -> max_transform_hierarchy_depth_intra = bs.getGolombU();

  psps -> scaling_list_enabled_flag = bs.getBits(1);
  if(psps -> scaling_list_enabled_flag)
  {
    psps -> sps_scaling_list_data_present_flag = bs.getBits(1);
    if(psps -> sps_scaling_list_data_present_flag)
    {
      psps -> scaling_list_data = processScalingListData(bs);
    }
  }

  psps -> amp_enabled_flag = bs.getBits(1);
  psps -> sample_adaptive_offset_enabled_flag = bs.getBits(1);
  psps -> pcm_enabled_flag = bs.getBits(1);

  if(psps -> pcm_enabled_flag)
  {
    psps -> pcm_sample_bit_depth_luma_minus1 = bs.getBits(4);
    psps -> pcm_sample_bit_depth_chroma_minus1 = bs.getBits(4);
    psps -> log2_min_pcm_luma_coding_block_size_minus3 = bs.getGolombU();
    psps -> log2_diff_max_min_pcm_luma_coding_block_size = bs.getGolombU();
    psps -> pcm_loop_filter_disabled_flag = bs.getBits(1);
  }

  psps -> num_short_term_ref_pic_sets = bs.getGolombU();

  psps -> short_term_ref_pic_set.resize(psps -> num_short_term_ref_pic_sets);
  for(std::size_t i=0; i<psps -> num_short_term_ref_pic_sets; i++)
    psps -> short_term_ref_pic_set[i] = processShortTermRefPicSet(i, psps -> num_short_term_ref_pic_sets, psps -> short_term_ref_pic_set, psps, bs);

  psps -> long_term_ref_pics_present_flag = bs.getBits(1);
  if(psps -> long_term_ref_pics_present_flag)
  {
    psps -> num_long_term_ref_pics_sps = bs.getGolombU();
    psps -> lt_ref_pic_poc_lsb_sps.resize(psps -> num_long_term_ref_pics_sps);
    psps -> used_by_curr_pic_lt_sps_flag.resize(psps -> num_long_term_ref_pics_sps);

    for(std::size_t i = 0; i<psps -> num_long_term_ref_pics_sps; i++)
    {
      psps -> lt_ref_pic_poc_lsb_sps[i] = bs.getBits(psps -> log2_max_pic_order_cnt_lsb_minus4 + 4);
      psps -> used_by_curr_pic_lt_sps_flag[i] = bs.getBits(1);
    }
  }

  psps -> sps_temporal_mvp_enabled_flag = bs.getBits(1);
  psps -> strong_intra_smoothing_enabled_flag = bs.getBits(1);
  psps -> vui_parameters_present_flag = bs.getBits(1);

  if(psps -> vui_parameters_present_flag)
  {
    psps -> vui_parameters = processVuiParameters(psps -> sps_max_sub_layers_minus1, bs);
  }

  psps -> sps_extension_flag = bs.getBits(1);
}

//=======================================================================

void HevcNalDecode::processPPS(std::shared_ptr<PPS> ppps, BitstreamReader &bs)
{
  ppps -> pps_pic_parameter_set_id = bs.getGolombU();
  ppps -> pps_seq_parameter_set_id  = bs.getGolombU();
  ppps -> dependent_slice_segments_enabled_flag = bs.getBits(1);

  ppps -> output_flag_present_flag = bs.getBits(1);
  ppps -> num_extra_slice_header_bits = bs.getBits(3);
  ppps -> sign_data_hiding_flag = bs.getBits(1);
  ppps -> cabac_init_present_flag = bs.getBits(1);
  ppps -> num_ref_idx_l0_default_active_minus1 = bs.getGolombU();
  ppps -> num_ref_idx_l1_default_active_minus1 = bs.getGolombU();
  ppps -> init_qp_minus26  = bs.getGolombS();
  ppps -> constrained_intra_pred_flag = bs.getBits(1);
  ppps -> transform_skip_enabled_flag = bs.getBits(1);
  ppps -> cu_qp_delta_enabled_flag = bs.getBits(1);

  if(ppps -> cu_qp_delta_enabled_flag)
    ppps -> diff_cu_qp_delta_depth = bs.getGolombU();
  else
    ppps -> diff_cu_qp_delta_depth = 0;

  ppps -> pps_cb_qp_offset = bs.getGolombS();
  ppps -> pps_cr_qp_offset = bs.getGolombS();
  ppps -> pps_slice_chroma_qp_offsets_present_flag = bs.getBits(1);
  ppps -> weighted_pred_flag = bs.getBits(1);
  ppps -> weighted_bipred_flag = bs.getBits(1);
  ppps -> transquant_bypass_enabled_flag = bs.getBits(1);
  ppps -> tiles_enabled_flag = bs.getBits(1);
  ppps -> entropy_coding_sync_enabled_flag = bs.getBits(1);

  if(ppps -> tiles_enabled_flag)
  {
    ppps -> num_tile_columns_minus1 = bs.getGolombU();
    ppps -> num_tile_rows_minus1 = bs.getGolombU();
    ppps -> uniform_spacing_flag = bs.getBits(1);

    if(!ppps -> uniform_spacing_flag)
    {
      ppps -> column_width_minus1.resize(ppps -> num_tile_columns_minus1);
      for(std::size_t i=0; i<ppps -> num_tile_columns_minus1; i++)
        ppps -> column_width_minus1[i] = bs.getGolombU();

      ppps -> row_height_minus1.resize(ppps -> num_tile_rows_minus1);
      for(std::size_t i=0; i<ppps -> num_tile_rows_minus1; i++)
        ppps -> row_height_minus1[i] = bs.getGolombU();
    }
    ppps -> loop_filter_across_tiles_enabled_flag = bs.getBits(1);
  }
  else
  {
    ppps -> num_tile_columns_minus1 = 0;
    ppps -> num_tile_rows_minus1 = 0;
    ppps -> uniform_spacing_flag = 1;
    ppps -> loop_filter_across_tiles_enabled_flag = 1;
  }

  ppps -> pps_loop_filter_across_slices_enabled_flag = bs.getBits(1);
  ppps -> deblocking_filter_control_present_flag = bs.getBits(1);

  if(ppps -> deblocking_filter_control_present_flag)
  {
    ppps -> deblocking_filter_override_enabled_flag = bs.getBits(1);
    ppps -> pps_deblocking_filter_disabled_flag = bs.getBits(1);

    if(!ppps -> pps_deblocking_filter_disabled_flag)
    {
      ppps -> pps_beta_offset_div2 = bs.getGolombS();
      ppps -> pps_tc_offset_div2 = bs.getGolombS();
    }
    else
    {
      ppps -> pps_beta_offset_div2 = 0;
      ppps -> pps_tc_offset_div2 = 0;
    }
  }
  else
  {
    ppps -> deblocking_filter_override_enabled_flag = 0;
    ppps -> pps_deblocking_filter_disabled_flag = 0;
  }

  ppps -> pps_scaling_list_data_present_flag = bs.getBits(1);
  if(ppps -> pps_scaling_list_data_present_flag)
  {
    ppps -> scaling_list_data = processScalingListData(bs);
  }

  ppps -> lists_modification_present_flag = bs.getBits(1);
  ppps -> log2_parallel_merge_level_minus2 = bs.getGolombU();
  ppps -> slice_segment_header_extension_present_flag = bs.getBits(1);
  ppps -> pps_extension_flag = bs.getBits(1);
}

//=======================================================================

VuiParameters HevcNalDecode::processVuiParameters(std::size_t sps_max_sub_layers_minus1, BitstreamReader &bs)
{
  VuiParameters vui;

  vui.toDefault();

  vui.aspect_ratio_idc = 0;
  vui.sar_width = 0;
  vui.sar_height = 0;


  vui.aspect_ratio_info_present_flag = bs.getBits(1);

  if(vui.aspect_ratio_info_present_flag)
  {
    vui.aspect_ratio_idc = bs.getBits(8);

    if(vui.aspect_ratio_idc == 255) //EXTENDED_SAR
    {
      vui.sar_width = bs.getBits(16);
      vui.sar_height = bs.getBits(16);
    }
  }


  vui.overscan_info_present_flag = bs.getBits(1);
  if(vui.overscan_info_present_flag)
    vui.overscan_appropriate_flag = bs.getBits(1);

  vui.video_format = 5;
  vui.video_full_range_flag = 0;
  vui.colour_primaries = 2;
  vui.transfer_characteristics = 2;
  vui.matrix_coeffs = 2;

  vui.video_signal_type_present_flag = bs.getBits(1);

  if(vui.video_signal_type_present_flag)
  {
    vui.video_format = bs.getBits(3);
    vui.video_full_range_flag = bs.getBits(1);
    vui.colour_description_present_flag = bs.getBits(1);

    if(vui.colour_description_present_flag)
    {
      vui.colour_primaries = bs.getBits(8);
      vui.transfer_characteristics = bs.getBits(8);
      vui.matrix_coeffs = bs.getBits(8);
    }

  }

  vui.chroma_sample_loc_type_top_field = 0;
  vui.chroma_sample_loc_type_bottom_field = 0;

  vui.chroma_loc_info_present_flag = bs.getBits(1);
  if(vui.chroma_loc_info_present_flag)
  {
    vui.chroma_sample_loc_type_top_field = bs.getGolombU();
    vui.chroma_sample_loc_type_bottom_field = bs.getGolombU();
  }


  vui.neutral_chroma_indication_flag = bs.getBits(1);
  vui.field_seq_flag = bs.getBits(1);
  vui.frame_field_info_present_flag = bs.getBits(1);
  vui.default_display_window_flag = bs.getBits(1);

  vui.def_disp_win_left_offset = 0;
  vui.def_disp_win_right_offset = 0;
  vui.def_disp_win_right_offset = 0;
  vui.def_disp_win_bottom_offset = 0;

  if(vui.default_display_window_flag)
  {
    vui.def_disp_win_left_offset = bs.getGolombU();
    vui.def_disp_win_right_offset = bs.getGolombU();
    vui.def_disp_win_top_offset = bs.getGolombU();
    vui.def_disp_win_bottom_offset = bs.getGolombU();
  }

  vui.vui_timing_info_present_flag = bs.getBits(1);

  if(vui.vui_timing_info_present_flag)
  {
    vui.vui_num_units_in_tick = bs.getBits(32);
    vui.vui_time_scale = bs.getBits(32);
    vui.vui_poc_proportional_to_timing_flag = bs.getBits(1);

    if(vui.vui_poc_proportional_to_timing_flag)
      vui.vui_num_ticks_poc_diff_one_minus1 = bs.getGolombU();

    vui.vui_hrd_parameters_present_flag = bs.getBits(1);

    if(vui.vui_hrd_parameters_present_flag)
      vui.hrd_parameters = processHrdParameters(1, sps_max_sub_layers_minus1, bs);
  }

  vui.bitstream_restriction_flag = bs.getBits(1);

  if(vui.bitstream_restriction_flag)
  {
    vui.tiles_fixed_structure_flag = bs.getBits(1);
    vui.motion_vectors_over_pic_boundaries_flag = bs.getBits(1);
    vui.restricted_ref_pic_lists_flag = bs.getBits(1);

    vui.min_spatial_segmentation_idc = bs.getGolombU();
    vui.max_bytes_per_pic_denom = bs.getGolombU();
    vui.max_bits_per_min_cu_denom = bs.getGolombU();
    vui.log2_max_mv_length_horizontal = bs.getGolombU();
    vui.log2_max_mv_length_vertical = bs.getGolombU();
  }

  return vui;
}

//=======================================================================

ProfileTierLevel HevcNalDecode::processProfileTierLevel(std::size_t max_sub_layers_minus1, BitstreamReader &bs)
{
  ProfileTierLevel ptl;

  ptl.toDefault();

  ptl.general_profile_space = bs.getBits(2);
  ptl.general_tier_flag = bs.getBits(1);
  ptl.general_profile_idc = bs.getBits(5);

  for(std::size_t i=0; i<32; i++)
    ptl.general_profile_compatibility_flag[i] = bs.getBits(1);

  ptl.general_progressive_source_flag = bs.getBits(1);
  ptl.general_interlaced_source_flag = bs.getBits(1);
  ptl.general_non_packed_constraint_flag = bs.getBits(1);
  ptl.general_frame_only_constraint_flag = bs.getBits(1);
  bs.getBits(32);
  bs.getBits(12);
  ptl.general_level_idc = bs.getBits(8);

  ptl.sub_layer_profile_present_flag.resize(max_sub_layers_minus1);
  ptl.sub_layer_level_present_flag.resize(max_sub_layers_minus1);

  for(std::size_t i=0; i<max_sub_layers_minus1; i++)
  {
    ptl.sub_layer_profile_present_flag[i] = bs.getBits(1);
    ptl.sub_layer_level_present_flag[i] = bs.getBits(1);
  }


  if(max_sub_layers_minus1 > 0)
  {
    for(std::size_t i=max_sub_layers_minus1; i<8; i++)
      bs.getBits(2);
  }

  ptl.sub_layer_profile_space.resize(max_sub_layers_minus1);
  ptl.sub_layer_tier_flag.resize(max_sub_layers_minus1);
  ptl.sub_layer_profile_idc.resize(max_sub_layers_minus1);
  ptl.sub_layer_profile_compatibility_flag.resize(max_sub_layers_minus1);
  ptl.sub_layer_progressive_source_flag.resize(max_sub_layers_minus1);
  ptl.sub_layer_interlaced_source_flag.resize(max_sub_layers_minus1);
  ptl.sub_layer_non_packed_constraint_flag.resize(max_sub_layers_minus1);
  ptl.sub_layer_frame_only_constraint_flag.resize(max_sub_layers_minus1);
  ptl.sub_layer_level_idc.resize(max_sub_layers_minus1);

  for(std::size_t i=0; i<max_sub_layers_minus1; i++)
  {
    if(ptl.sub_layer_profile_present_flag[i])
    {
      ptl.sub_layer_profile_space[i] = bs.getBits(2);
      ptl.sub_layer_tier_flag[i] = bs.getBits(1);
      ptl.sub_layer_profile_idc[i] = bs.getBits(5);
      ptl.sub_layer_profile_compatibility_flag[i].resize(32);

      for(std::size_t j=0; j<32; j++)
        ptl.sub_layer_profile_compatibility_flag[i][j] = bs.getBits(1);

      ptl.sub_layer_progressive_source_flag[i] = bs.getBits(1);
      ptl.sub_layer_interlaced_source_flag[i] = bs.getBits(1);
      ptl.sub_layer_non_packed_constraint_flag[i] = bs.getBits(1);
      ptl.sub_layer_frame_only_constraint_flag[i] = bs.getBits(1);
      bs.getBits(32);
      bs.getBits(12);

    }

    if(ptl.sub_layer_level_present_flag[i])
    {
      ptl.sub_layer_level_idc[i] = bs.getBits(8);
    }
    else
      ptl.sub_layer_level_idc[i] = 1;

  }

  return ptl;
}

//=======================================================================

ScalingListData HevcNalDecode::processScalingListData(BitstreamReader &bs)
{
  ScalingListData sc;

  sc.scaling_list_pred_mode_flag.resize(4);
  sc.scaling_list_pred_matrix_id_delta.resize(4);
  sc.scaling_list_dc_coef_minus8.resize(2);
  sc.scaling_list_delta_coef.resize(4);

  for(std::size_t sizeId = 0; sizeId < 4; sizeId++)
  {
    if(sizeId == 3)
    {
      sc.scaling_list_pred_mode_flag[sizeId].resize(2);
      sc.scaling_list_pred_matrix_id_delta[sizeId].resize(2);
      sc.scaling_list_dc_coef_minus8[sizeId-2].resize(2);
      sc.scaling_list_delta_coef[sizeId].resize(2);
    }
    else
    {
      sc.scaling_list_pred_mode_flag[sizeId].resize(6);
      sc.scaling_list_pred_matrix_id_delta[sizeId].resize(6);
      sc.scaling_list_delta_coef[sizeId].resize(6);
      if(sizeId >= 2)
        sc.scaling_list_dc_coef_minus8[sizeId-2].resize(6);
    }

    for(std::size_t matrixId = 0; matrixId<((sizeId == 3)?2:6); matrixId++)
    {
      sc.scaling_list_pred_mode_flag[sizeId][matrixId] = bs.getBits(1);
      if(!sc.scaling_list_pred_mode_flag[sizeId][matrixId])
        sc.scaling_list_pred_matrix_id_delta[sizeId][matrixId] = bs.getGolombU();
      else
      {
        std::size_t nextCoef = 8;
        std::size_t coefNum = std::min(64, (1 << (4 + (sizeId << 1))));
        if(sizeId > 1)
          sc.scaling_list_dc_coef_minus8[sizeId-2][matrixId] = bs.getGolombS();

        sc.scaling_list_delta_coef[sizeId][matrixId].resize(coefNum);
        for(std::size_t i = 0; i < coefNum; i++)
          sc.scaling_list_delta_coef[sizeId][matrixId][i] = bs.getGolombS();
      }
    }
  }

  return sc;
}

//=======================================================================

ShortTermRefPicSet HevcNalDecode::processShortTermRefPicSet(std::size_t stRpsIdx, std::size_t num_short_term_ref_pic_sets, const std::vector<ShortTermRefPicSet> &refPicSets, std::shared_ptr<SPS> psps, BitstreamReader &bs)
{
  ShortTermRefPicSet rpset;

  rpset.toDefault();

  rpset.inter_ref_pic_set_prediction_flag = 0;
  rpset.delta_idx_minus1 = 0;
  if(stRpsIdx)
  {
    rpset.inter_ref_pic_set_prediction_flag = bs.getBits(1);
  }

  if(rpset.inter_ref_pic_set_prediction_flag)
  {
    if(stRpsIdx == num_short_term_ref_pic_sets)
      rpset.delta_idx_minus1 = bs.getGolombU();

    rpset.delta_rps_sign = bs.getBits(1);
    rpset.abs_delta_rps_minus1 = bs.getGolombU();

    std::size_t RefRpsIdx = stRpsIdx - (rpset.delta_idx_minus1 + 1);
    std::size_t NumDeltaPocs = 0;

    if(refPicSets[RefRpsIdx].inter_ref_pic_set_prediction_flag)
    {
      for(std::size_t i=0; i<refPicSets[RefRpsIdx].used_by_curr_pic_flag.size(); i++)
        if(refPicSets[RefRpsIdx].used_by_curr_pic_flag[i] || refPicSets[RefRpsIdx].use_delta_flag[i])
          NumDeltaPocs++;
    }
    else
      NumDeltaPocs = refPicSets[RefRpsIdx].num_negative_pics + refPicSets[RefRpsIdx].num_positive_pics;

    rpset.used_by_curr_pic_flag.resize(NumDeltaPocs + 1);
    rpset.use_delta_flag.resize(NumDeltaPocs + 1, 1);

    for(std::size_t i=0; i<=NumDeltaPocs; i++ )
    {
      rpset.used_by_curr_pic_flag[i] = bs.getBits(1);
      if(!rpset.used_by_curr_pic_flag[i])
        rpset.use_delta_flag[i] = bs.getBits(1);
    }
  }
  else
  {
    rpset.num_negative_pics = bs.getGolombU();
    rpset.num_positive_pics = bs.getGolombU();

    if(rpset.num_negative_pics > psps -> sps_max_dec_pic_buffering_minus1[psps -> sps_max_sub_layers_minus1])
    {
      //onWarning("ShortTermRefPicSet: num_negative_pics > sps_max_dec_pic_buffering_minus1", &info, Parser::OUT_OF_RANGE);
      return rpset;
    }

    if(rpset.num_positive_pics > psps -> sps_max_dec_pic_buffering_minus1[psps -> sps_max_sub_layers_minus1])
    {
      //onWarning("ShortTermRefPicSet: num_positive_pics > sps_max_dec_pic_buffering_minus1", &info, Parser::OUT_OF_RANGE);
      return rpset;      
    }

    rpset.delta_poc_s0_minus1.resize(rpset.num_negative_pics);
    rpset.used_by_curr_pic_s0_flag.resize(rpset.num_negative_pics);

    for(std::size_t i=0; i<rpset.num_negative_pics; i++)
    {
      rpset.delta_poc_s0_minus1[i] = bs.getGolombU();
      rpset.used_by_curr_pic_s0_flag[i] = bs.getBits(1);
    }

    rpset.delta_poc_s1_minus1.resize(rpset.num_positive_pics);
    rpset.used_by_curr_pic_s1_flag.resize(rpset.num_positive_pics);
    for(std::size_t i=0; i<rpset.num_positive_pics; i++)
    {
      rpset.delta_poc_s1_minus1[i] = bs.getGolombU();
      rpset.used_by_curr_pic_s1_flag[i] = bs.getBits(1);
    }

  }

  return rpset;
}

//=======================================================================

HrdParameters HevcNalDecode::processHrdParameters(uint8_t commonInfPresentFlag, std::size_t maxNumSubLayersMinus1, BitstreamReader &bs)
{
  HrdParameters hrd;

  hrd.toDefault();

  hrd.nal_hrd_parameters_present_flag = 0;
  hrd.vcl_hrd_parameters_present_flag = 0;
  hrd.sub_pic_hrd_params_present_flag = 0;
  hrd.sub_pic_cpb_params_in_pic_timing_sei_flag = 0;
  if(commonInfPresentFlag)
  {
    hrd.nal_hrd_parameters_present_flag = bs.getBits(1);
    hrd.vcl_hrd_parameters_present_flag = bs.getBits(1);

    if(hrd.nal_hrd_parameters_present_flag || hrd.vcl_hrd_parameters_present_flag)
    {
      hrd.sub_pic_hrd_params_present_flag = bs.getBits(1);
      if(hrd.sub_pic_hrd_params_present_flag)
      {
        hrd.tick_divisor_minus2 = bs.getBits(8);
        hrd.du_cpb_removal_delay_increment_length_minus1 = bs.getBits(5);
        hrd.sub_pic_cpb_params_in_pic_timing_sei_flag = bs.getBits(1);
        hrd.dpb_output_delay_du_length_minus1 = bs.getBits(5);
      }
      hrd.bit_rate_scale = bs.getBits(4);
      hrd.cpb_size_scale = bs.getBits(4);

      if(hrd.sub_pic_hrd_params_present_flag)
        hrd.cpb_size_du_scale = bs.getBits(4);

      hrd.initial_cpb_removal_delay_length_minus1 = bs.getBits(5);
      hrd.au_cpb_removal_delay_length_minus1 = bs.getBits(5);
      hrd.dpb_output_delay_length_minus1 = bs.getBits(5);
    }
  }

  hrd.fixed_pic_rate_general_flag.resize(maxNumSubLayersMinus1 + 1);
  hrd.fixed_pic_rate_within_cvs_flag.resize(maxNumSubLayersMinus1 + 1);
  hrd.elemental_duration_in_tc_minus1.resize(maxNumSubLayersMinus1 + 1);
  hrd.low_delay_hrd_flag.resize(maxNumSubLayersMinus1 + 1, 0);
  hrd.cpb_cnt_minus1.resize(maxNumSubLayersMinus1 + 1, 0);

  if(hrd.nal_hrd_parameters_present_flag)
    hrd.nal_sub_layer_hrd_parameters.resize(maxNumSubLayersMinus1 + 1);
  if(hrd.vcl_hrd_parameters_present_flag)
    hrd.vcl_sub_layer_hrd_parameters.resize(maxNumSubLayersMinus1 + 1);

  for(std::size_t i = 0; i <= maxNumSubLayersMinus1; i++ )
  {
    hrd.fixed_pic_rate_general_flag[i] = bs.getBits(1);

    if(hrd.fixed_pic_rate_general_flag[i])
      hrd.fixed_pic_rate_within_cvs_flag[i] = 1;

    if(!hrd.fixed_pic_rate_general_flag[i])
       hrd.fixed_pic_rate_within_cvs_flag[i] = bs.getBits(1);

    if(hrd.fixed_pic_rate_within_cvs_flag[i])
      hrd.elemental_duration_in_tc_minus1[i] = bs.getGolombU();
    else
      hrd.low_delay_hrd_flag[i] = bs.getBits(1);

    if(!hrd.low_delay_hrd_flag[i])
      hrd.cpb_cnt_minus1[i] = bs.getGolombU();

    if(hrd.nal_hrd_parameters_present_flag)
      hrd.nal_sub_layer_hrd_parameters[i] = processSubLayerHrdParameters(hrd.sub_pic_hrd_params_present_flag, hrd.cpb_cnt_minus1[i], bs);
    if(hrd.vcl_hrd_parameters_present_flag)
      hrd.vcl_sub_layer_hrd_parameters[i] = processSubLayerHrdParameters(hrd.sub_pic_hrd_params_present_flag, hrd.cpb_cnt_minus1[i], bs);
  }

  return hrd;
}

//=======================================================================

SubLayerHrdParameters HevcNalDecode::processSubLayerHrdParameters(uint8_t sub_pic_hrd_params_present_flag, std::size_t CpbCnt, BitstreamReader &bs)
{
  SubLayerHrdParameters slhrd;

  slhrd.toDefault();
  slhrd.bit_rate_value_minus1.resize(CpbCnt + 1);
  slhrd.cpb_size_value_minus1.resize(CpbCnt + 1);
  slhrd.cpb_size_du_value_minus1.resize(CpbCnt + 1);
  slhrd.bit_rate_du_value_minus1.resize(CpbCnt + 1);
  slhrd.cbr_flag.resize(CpbCnt + 1);

  for(std::size_t i=0; i<=CpbCnt; i++)
  {
    slhrd.bit_rate_value_minus1[i] = bs.getGolombU();
    slhrd.cpb_size_value_minus1[i] = bs.getGolombU();

    if(sub_pic_hrd_params_present_flag)
    {
      slhrd.cpb_size_du_value_minus1[i] = bs.getGolombU();
      slhrd.bit_rate_du_value_minus1[i] = bs.getGolombU();
    }

    slhrd.cbr_flag[i] = bs.getBits(1);
  }

  return slhrd;
}
