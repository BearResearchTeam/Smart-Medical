<template>
  <div class="app-container">
    <!-- ... 搜索栏 ... -->
    <div class="search-container">
      <el-form :model="queryParams" ref="queryFormRef" :inline="true">
        <el-form-item label="药品名称" prop="drugName">
          <el-input v-model="queryParams.drugName" placeholder="请输入药品名称" clearable style="width: 200px" @keyup.enter="handleQuery" />
        </el-form-item>
        <el-form-item label="药品类型" prop="drugType">
          <el-input v-model="queryParams.drugType" placeholder="请输入药品类型" clearable style="width: 200px" @keyup.enter="handleQuery" />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="handleQuery"><i-ep-search />搜索</el-button>
          <el-button @click="resetQuery"><i-ep-refresh />重置</el-button>
        </el-form-item>
      </el-form>
    </div>
    
    <el-card>
      <!-- ... 操作栏 ... -->
      <template #header>
        <el-button type="success" @click="handleAdd"><i-ep-plus />新增</el-button>
      </template>

      <!-- 表格（已更新） -->
      <el-table v-loading="loading" :data="drugList" border>
        <el-table-column label="药品名称" prop="drugName" fixed="left" width="150" />
        <el-table-column label="药品类型" prop="drugType" width="120" />
        <el-table-column label="收费项目" prop="feeName" width="120" />
        <el-table-column label="剂型" prop="dosageForm" width="100" />
        <el-table-column label="规格" prop="specification" width="150" />
        <el-table-column label="进价" prop="purchasePrice" width="100" />
        <el-table-column label="售价" prop="salePrice" width="100" />
        <el-table-column label="库存" prop="stock" width="100" />
        <el-table-column label="库存上限" prop="stockUpper" width="100" />
        <el-table-column label="库存下限" prop="stockLower" width="100" />
        <el-table-column label="生产日期" prop="productionDate" width="180" />
        <el-table-column label="有效期至" prop="expiryDate" width="180" />
        <el-table-column label="功效" prop="effect" width="200" />
        <el-table-column label="分类" prop="category" width="100" />
        <el-table-column label="操作" align="center" width="180" fixed="right">
          <template #default="scope">
            <el-button type="primary" link @click="handleUpdate(scope.row)"><i-ep-edit />修改</el-button>
            <el-button type="danger" link @click="handleDelete(scope.row)"><i-ep-delete />删除</el-button>
          </template>
        </el-table-column>
      </el-table>

      <!-- ... 分页 ... -->
      <pagination
        v-if="total > 0"
        v-model:page="queryParams.pageIndex"
        v-model:limit="queryParams.pageSize"
        :total="total"
        @pagination="fetchData"
      />
    </el-card>

    <!-- ... 弹窗 ... -->
  </div>
</template>

<script setup lang="ts">
// ... script部分无需修改 ...
import { ref, onMounted, reactive } from 'vue';
import { ElMessage, ElMessageBox } from 'element-plus';
import DrugAPI from '@/api/drug'; // 假设你的API在 @/api/drug.js
import Pagination from '@/components/Pagination/index.vue';

const loading = ref(false);
const total = ref(0);
const drugList = ref<any[]>([]);

const queryParams = reactive({
  pageIndex: 1,
  pageSize: 10,
  drugName: '',
  drugType: '',
});

async function fetchData() {
  loading.value = true;
  try {
    const result = await DrugAPI.getDrugList(queryParams);
    if (result && result.data && result.data.data) {
      drugList.value = result.data.data.data || [];
      total.value = result.data.data.totleCount || 0;
    } else {
      drugList.value = [];
      total.value = 0;
    }
  } catch (error) {
    console.error("获取药品列表失败:", error);
    drugList.value = [];
    total.value = 0;
  } finally {
    loading.value = false;
  }
}

function handleQuery() {
  queryParams.pageIndex = 1;
  fetchData();
}

function resetQuery() {
  queryParams.pageIndex = 1;
  queryParams.drugName = '';
  queryParams.drugType = '';
  handleQuery();
}

function handleAdd() {
  ElMessage.info('新增功能待实现');
}

function handleUpdate(row: any) {
  ElMessage.info(`修改ID为 ${row.id} 的药品`);
}

function handleDelete(row: any) {
  ElMessageBox.confirm(`是否确认删除药品「${row.drugName}」?`, '警告', {
    confirmButtonText: '确定',
    cancelButtonText: '取消',
    type: 'warning',
  }).then(() => {
    ElMessage.info('删除功能待实现');
  }).catch(() => {});
}

onMounted(() => {
  fetchData();
});
</script> 