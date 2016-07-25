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

#include "Hevc.h"

#include <string.h>

using namespace HEVC;

NALUnit::NALUnit(NALUnitType type):
  m_nalUnitType(type)
  ,m_processFailed(false)
{
}


NALUnit::~NALUnit()
{
}


NALUnitType NALUnit::getType() const
{
  return m_nalUnitType;
}
 
 

HEVC::VPS::VPS(): NALUnit(HEVC::NAL_VPS) 
{ 
  toDefault();
}


HEVC::SPS::SPS(): NALUnit(NAL_SPS) 
{
  toDefault();
}


HEVC::PPS::PPS(): NALUnit(NAL_PPS) 
{
  toDefault();
};


HEVC::AUD::AUD(): NALUnit(NAL_AUD) 
{
  toDefault();
};


void ProfileTierLevel::toDefault()
{
    general_profile_space = 0;  
    general_tier_flag = 0;
    general_profile_idc = 0;
    general_profile_compatibility_flag[32];
    general_progressive_source_flag = 0;
    general_interlaced_source_flag = 0;
    general_non_packed_constraint_flag = 0;
    general_frame_only_constraint_flag = 0;
    general_level_idc = 0;
    sub_layer_profile_present_flag.clear();
    sub_layer_level_present_flag.clear();
    sub_layer_profile_space.clear();
    sub_layer_tier_flag.clear();
    sub_layer_profile_idc.clear();
    sub_layer_profile_compatibility_flag.clear();
    sub_layer_progressive_source_flag.clear();
    sub_layer_interlaced_source_flag.clear();
    sub_layer_non_packed_constraint_flag.clear();
    sub_layer_frame_only_constraint_flag.clear();
    sub_layer_level_idc.clear();
}


void SubLayerHrdParameters::toDefault()
{
    bit_rate_value_minus1.clear();
    cpb_size_value_minus1.clear();
    cpb_size_du_value_minus1.clear();
    bit_rate_du_value_minus1.clear();
    cbr_flag.clear();
}


void HrdParameters::toDefault()
{
  nal_hrd_parameters_present_flag = 0;
  vcl_hrd_parameters_present_flag = 0;
  sub_pic_hrd_params_present_flag = 0;
  tick_divisor_minus2 = 0;
  du_cpb_removal_delay_increment_length_minus1 = 0;
  sub_pic_cpb_params_in_pic_timing_sei_flag = 0;
  dpb_output_delay_du_length_minus1 = 0;
  bit_rate_scale = 0;
  cpb_size_scale = 0;
  cpb_size_du_scale = 0;
  initial_cpb_removal_delay_length_minus1 = 23;
  au_cpb_removal_delay_length_minus1 = 23;
  dpb_output_delay_length_minus1 = 23;
  fixed_pic_rate_general_flag.clear();
  fixed_pic_rate_within_cvs_flag.clear();
  elemental_duration_in_tc_minus1.clear();
  low_delay_hrd_flag.clear();
  cpb_cnt_minus1.clear();
  nal_sub_layer_hrd_parameters.clear();
  vcl_sub_layer_hrd_parameters.clear();
}


void ShortTermRefPicSet::toDefault()
{
    inter_ref_pic_set_prediction_flag = 0;
    delta_idx_minus1 = 0;
    delta_rps_sign = 0;
    abs_delta_rps_minus1 = 0;
    used_by_curr_pic_flag.clear();
    use_delta_flag.clear();
    num_negative_pics = 0;
    num_positive_pics = 0;
    delta_poc_s0_minus1.clear();
    used_by_curr_pic_s0_flag.clear();
    delta_poc_s1_minus1.clear();
    used_by_curr_pic_s1_flag.clear();
}



void VuiParameters::toDefault()
{
    aspect_ratio_info_present_flag = 0;
    aspect_ratio_idc = 0;
    sar_width = 0;
    sar_height = 0;
    overscan_info_present_flag = 0;
    overscan_appropriate_flag = 0;
    video_signal_type_present_flag = 0;
    video_format = 5;
    video_full_range_flag = 0;
    colour_description_present_flag = 0;
    colour_primaries = 2;
    transfer_characteristics = 2;
    matrix_coeffs = 2;
    chroma_loc_info_present_flag = 0;
    chroma_sample_loc_type_top_field = 0;
    chroma_sample_loc_type_bottom_field = 0;
    neutral_chroma_indication_flag = 0;
    field_seq_flag = 0;
    frame_field_info_present_flag = 0;
    default_display_window_flag = 0;
    def_disp_win_left_offset = 0;
    def_disp_win_right_offset = 0;
    def_disp_win_top_offset = 0;
    def_disp_win_bottom_offset = 0;
    vui_timing_info_present_flag = 0;
    vui_num_units_in_tick = 0;
    vui_time_scale = 0;
    vui_poc_proportional_to_timing_flag = 0;
    vui_num_ticks_poc_diff_one_minus1 = 0;
    vui_hrd_parameters_present_flag = 0;
    hrd_parameters.toDefault();
    bitstream_restriction_flag = 0;
    tiles_fixed_structure_flag = 0;
    motion_vectors_over_pic_boundaries_flag = 0;
    restricted_ref_pic_lists_flag = 0;
    min_spatial_segmentation_idc = 0;
    max_bytes_per_pic_denom = 2;
    max_bits_per_min_cu_denom = 1;
    log2_max_mv_length_horizontal = 15;
    log2_max_mv_length_vertical = 15;  
}



void VPS::toDefault()
{
  vps_video_parameter_set_id = 0;
  vps_max_layers_minus1 = 0;
  vps_max_sub_layers_minus1 = 0;
  vps_temporal_id_nesting_flag = 0;
  profile_tier_level.toDefault();
  vps_sub_layer_ordering_info_present_flag = 0;
  vps_max_dec_pic_buffering_minus1.clear();
  vps_max_num_reorder_pics.clear();
  vps_max_latency_increase_plus1.clear();
  vps_max_layer_id = 0;
  vps_num_layer_sets_minus1 = 0;
  layer_id_included_flag.clear();
  vps_timing_info_present_flag = 0;
  vps_num_units_in_tick = 0;
  vps_time_scale = 0;
  vps_poc_proportional_to_timing_flag = 0;
  vps_num_ticks_poc_diff_one_minus1 = 0;
  vps_num_hrd_parameters = 0;
  hrd_layer_set_idx.clear();
  cprms_present_flag.clear();
  hrd_parameters.clear();
  vps_extension_flag = 0;
}


void SPS::toDefault()
{
  sps_video_parameter_set_id = 0;
  sps_max_sub_layers_minus1 = 0;
  sps_temporal_id_nesting_flag = 0;
  profile_tier_level.toDefault();
  sps_seq_parameter_set_id = 0;
  chroma_format_idc = 0;
  separate_colour_plane_flag = 0;
  pic_width_in_luma_samples = 0;
  pic_height_in_luma_samples = 0;
  conformance_window_flag = 0;
  conf_win_left_offset = 0;
  conf_win_right_offset = 0;
  conf_win_top_offset = 0;
  conf_win_bottom_offset = 0;
  bit_depth_luma_minus8 = 0;
  bit_depth_chroma_minus8 = 0;
  log2_max_pic_order_cnt_lsb_minus4 = 0;
  sps_sub_layer_ordering_info_present_flag = 0;
  sps_max_dec_pic_buffering_minus1.clear();
  sps_max_num_reorder_pics.clear();
  sps_max_latency_increase_plus1.clear();
  log2_min_luma_coding_block_size_minus3 = 0;
  log2_diff_max_min_luma_coding_block_size = 0;
  log2_min_transform_block_size_minus2 = 0;
  log2_diff_max_min_transform_block_size = 0;
  max_transform_hierarchy_depth_inter = 0;
  max_transform_hierarchy_depth_intra = 0;
  scaling_list_enabled_flag = 0;
  scaling_list_data.toDefault();
  sps_scaling_list_data_present_flag = 0;
  amp_enabled_flag = 0;
  sample_adaptive_offset_enabled_flag = 0;
  pcm_enabled_flag = 0;
  pcm_sample_bit_depth_luma_minus1 = 0;
  pcm_sample_bit_depth_chroma_minus1 = 0;
  log2_min_pcm_luma_coding_block_size_minus3 = 0;
  log2_diff_max_min_pcm_luma_coding_block_size = 0;
  pcm_loop_filter_disabled_flag = 0;
  num_short_term_ref_pic_sets = 0;
  short_term_ref_pic_set.clear();
  long_term_ref_pics_present_flag = 0;
  num_long_term_ref_pics_sps = 0;
  lt_ref_pic_poc_lsb_sps.clear();
  used_by_curr_pic_lt_sps_flag.clear();
  sps_temporal_mvp_enabled_flag = 0;
  strong_intra_smoothing_enabled_flag = 0;
  vui_parameters_present_flag = 0;
  vui_parameters.toDefault();
  sps_extension_flag = 0;
}



void PPS::toDefault()
{
    pps_pic_parameter_set_id = 0;
    pps_seq_parameter_set_id = 0;
    dependent_slice_segments_enabled_flag = 0;
    output_flag_present_flag = 0;
    num_extra_slice_header_bits = 0;
    sign_data_hiding_flag = 0;
    cabac_init_present_flag = 0;
    num_ref_idx_l0_default_active_minus1 = 0;
    num_ref_idx_l1_default_active_minus1 = 0;
    init_qp_minus26 = 0;
    constrained_intra_pred_flag = 0;
    transform_skip_enabled_flag = 0;
    cu_qp_delta_enabled_flag = 0;
    diff_cu_qp_delta_depth = 0;
    pps_cb_qp_offset = 0;
    pps_cr_qp_offset = 0;
    pps_slice_chroma_qp_offsets_present_flag = 0;
    weighted_pred_flag = 0;
    weighted_bipred_flag = 0;
    transquant_bypass_enabled_flag = 0;
    tiles_enabled_flag = 0;
    entropy_coding_sync_enabled_flag = 0;
    num_tile_columns_minus1 = 0;
    num_tile_rows_minus1 = 0;
    uniform_spacing_flag = 1;
    column_width_minus1.clear();
    row_height_minus1.clear();
    loop_filter_across_tiles_enabled_flag = 0;
    pps_loop_filter_across_slices_enabled_flag = 0;
    deblocking_filter_control_present_flag = 0;
    deblocking_filter_override_enabled_flag = 0;
    pps_deblocking_filter_disabled_flag = 0;
    pps_beta_offset_div2 = 0;
    pps_tc_offset_div2 = 0;
    pps_scaling_list_data_present_flag = 0;
    lists_modification_present_flag = 0;
    log2_parallel_merge_level_minus2 = 0;
    slice_segment_header_extension_present_flag = 0;
    pps_extension_flag = 0;
}


void ScalingListData::toDefault()
{
  scaling_list_delta_coef.clear();
  scaling_list_pred_mode_flag.clear();
  scaling_list_pred_matrix_id_delta.clear();
  scaling_list_dc_coef_minus8.clear();
}


void AUD::toDefault()
{
  pic_type = 0;
}


