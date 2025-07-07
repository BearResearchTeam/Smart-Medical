<template>
  <div class="batch-delete-demo">
    <el-card>
      <div slot="header">
        <span>事务批量删除功能演示</span>
      </div>
      
      <!-- 药品批量删除 -->
      <el-card style="margin-bottom: 20px;">
        <div slot="header">
          <span>药品批量删除</span>
        </div>
        
        <el-table
          :data="drugList"
          @selection-change="handleDrugSelectionChange"
          v-loading="drugLoading"
          style="width: 100%">
          <el-table-column type="selection" width="55"></el-table-column>
          <el-table-column prop="drugID" label="药品ID" width="80"></el-table-column>
          <el-table-column prop="drugName" label="药品名称" width="150"></el-table-column>
          <el-table-column prop="stock" label="库存" width="80"></el-table-column>
          <el-table-column prop="expiryDate" label="有效期" width="120">
            <template slot-scope="scope">
              {{ formatDate(scope.row.expiryDate) }}
            </template>
          </el-table-column>
        </el-table>
        
        <div class="batch-actions" style="margin-top: 10px;">
          <el-button
            type="danger"
            :disabled="selectedDrugs.length === 0"
            @click="handleBatchDeleteDrugs(false)">
            批量删除药品
          </el-button>
          <el-button
            type="danger"
            :disabled="selectedDrugs.length === 0"
            @click="handleBatchDeleteDrugs(true)">
            强制批量删除药品
          </el-button>
          <el-button @click="loadDrugList">刷新药品列表</el-button>
        </div>
      </el-card>

      <!-- 病历批量删除 -->
      <el-card style="margin-bottom: 20px;">
        <div slot="header">
          <span>病历批量删除</span>
        </div>
        
        <el-table
          :data="medicalRecordList"
          @selection-change="handleMedicalRecordSelectionChange"
          v-loading="medicalRecordLoading"
          style="width: 100%">
          <el-table-column type="selection" width="55"></el-table-column>
          <el-table-column prop="id" label="病历ID" width="120"></el-table-column>
          <el-table-column prop="status" label="状态" width="100"></el-table-column>
          <el-table-column prop="creationTime" label="创建时间" width="180">
            <template slot-scope="scope">
              {{ formatDate(scope.row.creationTime) }}
            </template>
          </el-table-column>
        </el-table>
        
        <div class="batch-actions" style="margin-top: 10px;">
          <el-button
            type="danger"
            :disabled="selectedMedicalRecords.length === 0"
            @click="handleBatchDeleteMedicalRecords(false)">
            批量删除病历
          </el-button>
          <el-button
            type="danger"
            :disabled="selectedMedicalRecords.length === 0"
            @click="handleBatchDeleteMedicalRecords(true)">
            强制批量删除病历
          </el-button>
          <el-button @click="loadMedicalRecordList">刷新病历列表</el-button>
        </div>
      </el-card>

      <!-- 字典类型批量删除 -->
      <el-card style="margin-bottom: 20px;">
        <div slot="header">
          <span>字典类型批量删除</span>
        </div>
        
        <el-table
          :data="dictionaryTypeList"
          @selection-change="handleDictionaryTypeSelectionChange"
          v-loading="dictionaryTypeLoading"
          style="width: 100%">
          <el-table-column type="selection" width="55"></el-table-column>
          <el-table-column prop="id" label="类型ID" width="120"></el-table-column>
          <el-table-column prop="typeName" label="类型名称" width="150"></el-table-column>
          <el-table-column prop="typeCode" label="类型代码" width="120"></el-table-column>
        </el-table>
        
        <div class="batch-actions" style="margin-top: 10px;">
          <el-button
            type="danger"
            :disabled="selectedDictionaryTypes.length === 0"
            @click="handleBatchDeleteDictionaryTypes(false)">
            批量删除字典类型
          </el-button>
          <el-button
            type="danger"
            :disabled="selectedDictionaryTypes.length === 0"
            @click="handleBatchDeleteDictionaryTypes(true)">
            强制批量删除字典类型
          </el-button>
          <el-button @click="loadDictionaryTypeList">刷新字典类型列表</el-button>
        </div>
      </el-card>

      <!-- 科室批量删除 -->
      <el-card>
        <div slot="header">
          <span>科室批量删除</span>
        </div>
        
        <el-table
          :data="departmentList"
          @selection-change="handleDepartmentSelectionChange"
          v-loading="departmentLoading"
          style="width: 100%">
          <el-table-column type="selection" width="55"></el-table-column>
          <el-table-column prop="id" label="科室ID" width="120"></el-table-column>
          <el-table-column prop="departmentName" label="科室名称" width="150"></el-table-column>
          <el-table-column prop="status" label="状态" width="100"></el-table-column>
        </el-table>
        
        <div class="batch-actions" style="margin-top: 10px;">
          <el-button
            type="danger"
            :disabled="selectedDepartments.length === 0"
            @click="handleBatchDeleteDepartments(false)">
            批量删除科室
          </el-button>
          <el-button
            type="danger"
            :disabled="selectedDepartments.length === 0"
            @click="handleBatchDeleteDepartments(true)">
            强制批量删除科室
          </el-button>
          <el-button @click="loadDepartmentList">刷新科室列表</el-button>
        </div>
      </el-card>
    </el-card>
  </div>
</template>

<script>
import BatchDeleteAPI from '@/api/batch-delete'
import DrugAPI from '@/api/drug'

export default {
  name: 'BatchDeleteDemo',
  data() {
    return {
      // 药品相关
      drugList: [],
      selectedDrugs: [],
      drugLoading: false,
      
      // 病历相关
      medicalRecordList: [],
      selectedMedicalRecords: [],
      medicalRecordLoading: false,
      
      // 字典类型相关
      dictionaryTypeList: [],
      selectedDictionaryTypes: [],
      dictionaryTypeLoading: false,
      
      // 科室相关
      departmentList: [],
      selectedDepartments: [],
      departmentLoading: false
    }
  },
  mounted() {
    this.loadAllData()
  },
  methods: {
    // 加载所有数据
    async loadAllData() {
      await Promise.all([
        this.loadDrugList(),
        this.loadMedicalRecordList(),
        this.loadDictionaryTypeList(),
        this.loadDepartmentList()
      ])
    },

    // 加载药品列表
    async loadDrugList() {
      this.drugLoading = true
      try {
        const response = await DrugAPI.getDrugList({
          pageIndex: 1,
          pageSize: 50
        })
        if (response.success) {
          this.drugList = response.result.data
        } else {
          this.$message.error(response.message || '加载药品列表失败')
        }
      } catch (error) {
        console.error('加载药品列表失败:', error)
        this.$message.error('加载药品列表失败')
      } finally {
        this.drugLoading = false
      }
    },

    // 加载病历列表（模拟数据）
    async loadMedicalRecordList() {
      this.medicalRecordLoading = true
      try {
        // 这里应该调用实际的病历API
        // 暂时使用模拟数据
        this.medicalRecordList = [
          { id: '1', status: '已完成', creationTime: new Date() },
          { id: '2', status: '进行中', creationTime: new Date() }
        ]
      } catch (error) {
        console.error('加载病历列表失败:', error)
        this.$message.error('加载病历列表失败')
      } finally {
        this.medicalRecordLoading = false
      }
    },

    // 加载字典类型列表（模拟数据）
    async loadDictionaryTypeList() {
      this.dictionaryTypeLoading = true
      try {
        // 这里应该调用实际的字典类型API
        // 暂时使用模拟数据
        this.dictionaryTypeList = [
          { id: '1', typeName: '性别', typeCode: 'GENDER' },
          { id: '2', typeName: '血型', typeCode: 'BLOOD_TYPE' }
        ]
      } catch (error) {
        console.error('加载字典类型列表失败:', error)
        this.$message.error('加载字典类型列表失败')
      } finally {
        this.dictionaryTypeLoading = false
      }
    },

    // 加载科室列表（模拟数据）
    async loadDepartmentList() {
      this.departmentLoading = true
      try {
        // 这里应该调用实际的科室API
        // 暂时使用模拟数据
        this.departmentList = [
          { id: '1', departmentName: '内科', status: '启用' },
          { id: '2', departmentName: '外科', status: '启用' }
        ]
      } catch (error) {
        console.error('加载科室列表失败:', error)
        this.$message.error('加载科室列表失败')
      } finally {
        this.departmentLoading = false
      }
    },

    // 处理药品选择变化
    handleDrugSelectionChange(selection) {
      this.selectedDrugs = selection
    },

    // 处理病历选择变化
    handleMedicalRecordSelectionChange(selection) {
      this.selectedMedicalRecords = selection
    },

    // 处理字典类型选择变化
    handleDictionaryTypeSelectionChange(selection) {
      this.selectedDictionaryTypes = selection
    },

    // 处理科室选择变化
    handleDepartmentSelectionChange(selection) {
      this.selectedDepartments = selection
    },

    // 批量删除药品
    async handleBatchDeleteDrugs(forceDelete) {
      if (this.selectedDrugs.length === 0) {
        this.$message.warning('请选择要删除的药品')
        return
      }

      const drugIds = this.selectedDrugs.map(drug => drug.drugID)
      const drugNames = this.selectedDrugs.map(drug => drug.drugName).join(', ')
      
      const confirmMessage = forceDelete 
        ? `确定要强制删除以下药品吗？\n${drugNames}\n\n注意：强制删除将忽略库存和有效期检查！`
        : `确定要删除以下药品吗？\n${drugNames}`

      try {
        await this.$confirm(confirmMessage, '确认批量删除', {
          confirmButtonText: '确定',
          cancelButtonText: '取消',
          type: 'warning'
        })
        
        const data = {
          drugIds: drugIds,
          forceDelete: forceDelete
        }
        
        const response = await BatchDeleteAPI.batchDeleteDrugs(data)
        if (response.success) {
          this.$message.success(`成功删除 ${this.selectedDrugs.length} 个药品`)
          this.selectedDrugs = []
          this.loadDrugList()
        } else {
          this.$message.error(response.message || '批量删除失败')
        }
      } catch (error) {
        if (error !== 'cancel') {
          console.error('批量删除药品失败:', error)
          this.$message.error('批量删除药品失败')
        }
      }
    },

    // 批量删除病历
    async handleBatchDeleteMedicalRecords(forceDelete) {
      if (this.selectedMedicalRecords.length === 0) {
        this.$message.warning('请选择要删除的病历')
        return
      }

      const recordIds = this.selectedMedicalRecords.map(record => record.id)
      const recordNames = this.selectedMedicalRecords.map(record => record.id).join(', ')
      
      const confirmMessage = forceDelete 
        ? `确定要强制删除以下病历吗？\n${recordNames}\n\n注意：强制删除将忽略状态检查！`
        : `确定要删除以下病历吗？\n${recordNames}`

      try {
        await this.$confirm(confirmMessage, '确认批量删除', {
          confirmButtonText: '确定',
          cancelButtonText: '取消',
          type: 'warning'
        })
        
        const data = {
          ids: recordIds,
          forceDelete: forceDelete
        }
        
        const response = await BatchDeleteAPI.batchDeleteMedicalRecords(data)
        if (response.success) {
          this.$message.success(`成功删除 ${this.selectedMedicalRecords.length} 个病历`)
          this.selectedMedicalRecords = []
          this.loadMedicalRecordList()
        } else {
          this.$message.error(response.message || '批量删除失败')
        }
      } catch (error) {
        if (error !== 'cancel') {
          console.error('批量删除病历失败:', error)
          this.$message.error('批量删除病历失败')
        }
      }
    },

    // 批量删除字典类型
    async handleBatchDeleteDictionaryTypes(forceDelete) {
      if (this.selectedDictionaryTypes.length === 0) {
        this.$message.warning('请选择要删除的字典类型')
        return
      }

      const typeIds = this.selectedDictionaryTypes.map(type => type.id)
      const typeNames = this.selectedDictionaryTypes.map(type => type.typeName).join(', ')
      
      const confirmMessage = forceDelete 
        ? `确定要强制删除以下字典类型吗？\n${typeNames}\n\n注意：强制删除将忽略数据关联检查！`
        : `确定要删除以下字典类型吗？\n${typeNames}`

      try {
        await this.$confirm(confirmMessage, '确认批量删除', {
          confirmButtonText: '确定',
          cancelButtonText: '取消',
          type: 'warning'
        })
        
        const data = {
          ids: typeIds,
          forceDelete: forceDelete
        }
        
        const response = await BatchDeleteAPI.batchDeleteDictionaryTypes(data)
        if (response.success) {
          this.$message.success(`成功删除 ${this.selectedDictionaryTypes.length} 个字典类型`)
          this.selectedDictionaryTypes = []
          this.loadDictionaryTypeList()
        } else {
          this.$message.error(response.message || '批量删除失败')
        }
      } catch (error) {
        if (error !== 'cancel') {
          console.error('批量删除字典类型失败:', error)
          this.$message.error('批量删除字典类型失败')
        }
      }
    },

    // 批量删除科室
    async handleBatchDeleteDepartments(forceDelete) {
      if (this.selectedDepartments.length === 0) {
        this.$message.warning('请选择要删除的科室')
        return
      }

      const deptIds = this.selectedDepartments.map(dept => dept.id)
      const deptNames = this.selectedDepartments.map(dept => dept.departmentName).join(', ')
      
      const confirmMessage = forceDelete 
        ? `确定要强制删除以下科室吗？\n${deptNames}\n\n注意：强制删除将忽略状态检查！`
        : `确定要删除以下科室吗？\n${deptNames}`

      try {
        await this.$confirm(confirmMessage, '确认批量删除', {
          confirmButtonText: '确定',
          cancelButtonText: '取消',
          type: 'warning'
        })
        
        const data = {
          ids: deptIds,
          forceDelete: forceDelete
        }
        
        const response = await BatchDeleteAPI.batchDeleteDepartments(data)
        if (response.success) {
          this.$message.success(`成功删除 ${this.selectedDepartments.length} 个科室`)
          this.selectedDepartments = []
          this.loadDepartmentList()
        } else {
          this.$message.error(response.message || '批量删除失败')
        }
      } catch (error) {
        if (error !== 'cancel') {
          console.error('批量删除科室失败:', error)
          this.$message.error('批量删除科室失败')
        }
      }
    },

    // 格式化日期
    formatDate(date) {
      if (!date) return ''
      return new Date(date).toLocaleDateString()
    }
  }
}
</script>

<style scoped>
.batch-delete-demo {
  padding: 20px;
}

.batch-actions {
  display: flex;
  gap: 10px;
}
</style> 