#ifndef HEVC_NAL_DECODE
#define HEVC_NAL_DECODE

//#include "HevcParser.h"
//#include "HevcParser.h"
#include "Hevc.h"
#include "BitstreamReader.h"

#include <map>
#include <list>
#include <memory>

namespace HEVC
{
  //class HevcNalDecode: public Parser
  class HevcNalDecode
  {
    public:
      void processNALUnit(const uint8_t *pdata, std::size_t size, hevchdr& h);

    protected:
      NALUnitType processNALUnitHeader(BitstreamReader &bs);
      void processSPS(std::shared_ptr<SPS> psps, BitstreamReader &bs);
      void processPPS(std::shared_ptr<PPS> ppps, BitstreamReader &bs);
      ProfileTierLevel processProfileTierLevel(std::size_t max_sub_layers_minus1, BitstreamReader &bs);
      HrdParameters processHrdParameters(uint8_t commonInfPresentFlag, std::size_t maxNumSubLayersMinus1, BitstreamReader &bs);
      ShortTermRefPicSet processShortTermRefPicSet(std::size_t stRpsIdx, size_t num_short_term_ref_pic_sets, const std::vector<ShortTermRefPicSet> &refPicSets, std::shared_ptr<SPS> psps, BitstreamReader &bs);
      VuiParameters processVuiParameters(std::size_t sps_max_sub_layers_minus1, BitstreamReader &bs);
      ScalingListData processScalingListData(BitstreamReader &bs);
      SubLayerHrdParameters processSubLayerHrdParameters(uint8_t sub_pic_hrd_params_present_flag, std::size_t CpbCnt, BitstreamReader &bs);

      //void onWarning(const std::string &warning, const Info *pInfo, WarningType type);
  };
}

#endif

