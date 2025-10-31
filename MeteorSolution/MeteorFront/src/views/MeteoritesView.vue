<template>
  <div>
    <Filters @apply="fetchData" />
    <div v-if="error" class="error">{{ error }}</div>
    <MeteoritesTable :data="meteorites" />
  </div>
</template>

<script setup>
import { ref } from "vue";
import Filters from "/src/components/Filters.vue";
import MeteoritesTable from "/src/components/MeteoriteTable.vue";
import { getMeteorites } from "/src/api/meteorites.js";

const meteorites = ref([]);
const error = ref("");

async function fetchData(filters) {
  try {
    error.value = "";
    meteorites.value = await getMeteorites(filters);
  } catch (err) {
    error.value = err.message;
  }
}
</script>

<style scoped>
.error {
  color: red;
  margin-bottom: 1rem;
}
</style>
