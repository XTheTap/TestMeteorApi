<template>
  <div>
    <Filters @apply="fetchData" />
    <div v-if="error" class="error">{{ error }}</div>
    <MeteoritesTable :data="meteorites" @sort="onSort" />
  </div>
</template>

<script setup>
import { ref } from "vue";
import Filters from "/src/components/Filters.vue";
import MeteoritesTable from "/src/components/MeteoriteTable.vue";
import { getMeteorites } from "/src/api/meteorites.js";

const meteorites = ref([]);
const error = ref("");
let lastFilters = {};

async function fetchData(filters) {
  try {
    error.value = "";
    lastFilters = { ...filters };
    meteorites.value = await getMeteorites(filters);
  } catch (err) {
    error.value = err.message || String(err);
  }
}

async function onSort(sortBy) {
  // toggle sort direction if same column clicked twice
  const desc = lastFilters.Desc === true ? false : true;
  const filters = { ...lastFilters, SortBy: sortBy, Desc: desc };
  await fetchData(filters);
}
</script>

<style scoped>
.error {
  color: red;
  margin-bottom: 1rem;
}
</style>
