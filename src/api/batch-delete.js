import request from '@/utils/request';

// API地址
const api_name = '/api/app/batch-delete';

/**
 * 封装批量删除相关API
 */
const BatchDeleteAPI = {
  /**
   * 批量删除药品
   * @param data 包含drugIds数组和forceDelete布尔值
   */
  batchDeleteDrugs(data) {
    return request({
      url: `${api_name}/drugs`,
      method: 'post',
      data: data
    });
  },

  /**
   * 批量删除病历
   * @param data 包含ids数组和forceDelete布尔值
   */
  batchDeleteMedicalRecords(data) {
    return request({
      url: `${api_name}/medical-records`,
      method: 'post',
      data: data
    });
  },

  /**
   * 批量删除字典类型
   * @param data 包含ids数组和forceDelete布尔值
   */
  batchDeleteDictionaryTypes(data) {
    return request({
      url: `${api_name}/dictionary-types`,
      method: 'post',
      data: data
    });
  },

  /**
   * 批量删除科室
   * @param data 包含ids数组和forceDelete布尔值
   */
  batchDeleteDepartments(data) {
    return request({
      url: `${api_name}/departments`,
      method: 'post',
      data: data
    });
  }
};

export default BatchDeleteAPI; 