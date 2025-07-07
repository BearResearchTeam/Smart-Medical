<template>
  <div class="drug-batch-delete">
    <el-card>
      <div slot="header">
        <span>药品批量删除</span>
      </div>
      
      <!-- 药品列表 -->
      <el-table
        :data="drugList"
        @selection-change="handleSelectionChange"
        v-loading="loading"
        style="width: 100%">
        <el-table-column
          type="selection"
          width="55">
        </el-table-column>
        <el-table-column
          prop="drugID"
          label="药品ID"
          width="80">
        </el-table-column>
        <el-table-column
          prop="drugName"
          label="药品名称"
          width="150">
        </el-table-column>
        <el-table-column
          prop="drugType"
          label="药品类型"
          width="100">
        </el-table-column>
        <el-table-column
          prop="stock"
          label="库存"
          width="80">
        </el-table-column>
        <el-table-column
          prop="expiryDate"
          label="有效期"
          width="120">
          <template slot-scope="scope">
            {{ formatDate(scope.row.expiryDate) }}
          </template>
        </el-table-column>
        <el-table-column
          prop="pharmaceuticalCompanyName"
          label="供应商"
          width="150">
        </el-table-column>
        <el-table-column
          label="操作"
          width="120">
          <template slot-scope="scope">
            <el-button
              size="mini"
              type="danger"
              @click="handleDelete(scope.row)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>

      <!-- 批量操作按钮 -->
      <div class="batch-actions" style="margin-top: 20px;">
        <el-button
          type="danger"
          :disabled="selectedDrugs.length === 0"
          @click="handleBatchDelete(false)">
          批量删除
        </el-button>
        <el-button
          type="danger"
          :disabled="selectedDrugs.length === 0"
          @click="handleBatchDelete(true)">
          强制批量删除
        </el-button>
        <el-button @click="loadDrugList">刷新列表</el-button>
      </div>

      <!-- 分页 -->
      <el-pagination
        @size-change="handleSizeChange"
        @current-change="handleCurrentChange"
        :current-page="pagination.pageIndex"
        :page-sizes="[10, 20, 50, 100]"
        :page-size="pagination.pageSize"
        layout="total, sizes, prev, pager, next, jumper"
        :total="pagination.total"
        style="margin-top: 20px; text-align: right;">
      </el-pagination>
    </el-card>
  </div>
</template>

<script>
import DrugAPI from '@/api/drug'

export default {
  name: 'DrugBatchDelete',
  data() {
    return {
      loading: false,
      drugList: [],
      selectedDrugs: [],
      pagination: {
        pageIndex: 1,
        pageSize: 20,
        total: 0
      }
    }
  },
  mounted() {
    this.loadDrugList()
  },
  methods: {
    // 加载药品列表
    async loadDrugList() {
      this.loading = true
      try {
        const params = {
          pageIndex: this.pagination.pageIndex,
          pageSize: this.pagination.pageSize
        }
        const response = await DrugAPI.getDrugList(params)
        if (response.success) {
          this.drugList = response.result.data
          this.pagination.total = response.result.totleCount
        } else {
          this.$message.error(response.message || '加载药品列表失败')
        }
      } catch (error) {
        console.error('加载药品列表失败:', error)
        this.$message.error('加载药品列表失败')
      } finally {
        this.loading = false
      }
    },

    // 处理选择变化
    handleSelectionChange(selection) {
      this.selectedDrugs = selection
    },

    // 单个删除
    async handleDelete(drug) {
      try {
        await this.$confirm(`确定要删除药品"${drug.drugName}"吗？`, '确认删除', {
          confirmButtonText: '确定',
          cancelButtonText: '取消',
          type: 'warning'
        })
        
        const response = await DrugAPI.deleteDrug(drug.drugID)
        if (response.success) {
          this.$message.success('删除成功')
          this.loadDrugList()
        } else {
          this.$message.error(response.message || '删除失败')
        }
      } catch (error) {
        if (error !== 'cancel') {
          console.error('删除失败:', error)
          this.$message.error('删除失败')
        }
      }
    },

    // 批量删除
    async handleBatchDelete(forceDelete = false) {
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
        
        const response = await DrugAPI.batchDeleteDrug(data)
        if (response.success) {
          this.$message.success(`成功删除 ${this.selectedDrugs.length} 个药品`)
          this.selectedDrugs = []
          this.loadDrugList()
        } else {
          this.$message.error(response.message || '批量删除失败')
        }
      } catch (error) {
        if (error !== 'cancel') {
          console.error('批量删除失败:', error)
          this.$message.error('批量删除失败')
        }
      }
    },

    // 分页大小变化
    handleSizeChange(val) {
      this.pagination.pageSize = val
      this.pagination.pageIndex = 1
      this.loadDrugList()
    },

    // 当前页变化
    handleCurrentChange(val) {
      this.pagination.pageIndex = val
      this.loadDrugList()
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
.drug-batch-delete {
  padding: 20px;
}

.batch-actions {
  display: flex;
  gap: 10px;
}
</style> 