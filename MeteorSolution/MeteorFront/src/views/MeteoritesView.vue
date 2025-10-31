<template>
  <div>
    <Filters @apply="fetchData" />
    <div v-if="error" class="error">{{ error }}</div>
    <MeteoritesTable :data="meteorites" />
  </div>
</template>

<script setup>
import { ref } from "vue";
import Filters from "@/components/Filters.vue";
import MeteoritesTable from "@/components/MeteoritesTable.vue";
import { getMeteorites } from "@/api/meteorites.js";

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
