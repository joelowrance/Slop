# Database Migration Workflow

## Overview
Process for safely creating and applying database migrations in the VerdaVidaLawnCare application, ensuring data integrity and minimal downtime.

## Workflow Steps

### 1. Pre-Migration Planning
- **Review Changes**: Analyze what entities, properties, or relationships are changing
- **Impact Assessment**: Determine which tables and data will be affected
- **Backup Strategy**: Plan database backup before migration
- **Rollback Plan**: Prepare rollback strategy if migration fails
- **Downtime Window**: Schedule migration during low-usage periods

### 2. Entity Model Changes
- **Update Entity Classes**: Modify domain models as needed
- **Add/Remove Properties**: Update entity properties and attributes
- **Configure Relationships**: Update foreign keys and navigation properties
- **Add Indexes**: Consider performance implications of new queries
- **Data Annotations**: Ensure proper validation and constraints

### 3. Generate Migration
- **Create Migration**: Use `dotnet ef migrations add MigrationName`
- **Review Generated Code**: Check the generated migration script
- **Customize if Needed**: Add custom SQL for complex changes
- **Test Migration**: Run migration on development database
- **Verify Schema**: Ensure database schema matches expectations

### 4. Data Migration (if needed)
- **Identify Data Changes**: Determine if existing data needs transformation
- **Create Data Migration Scripts**: Write custom SQL for data updates
- **Test Data Migration**: Verify data transformation works correctly
- **Backup Critical Data**: Ensure important data is backed up

### 5. Testing Phase
- **Unit Tests**: Update tests to work with new schema
- **Integration Tests**: Test database operations with new structure
- **Performance Tests**: Ensure queries still perform well
- **Data Validation**: Verify data integrity after migration

### 6. Staging Deployment
- **Deploy to Staging**: Apply migration to staging environment
- **Test Application**: Verify application works with new schema
- **Performance Monitoring**: Check for performance regressions
- **User Acceptance Testing**: Test with realistic data and scenarios

### 7. Production Deployment
- **Schedule Maintenance Window**: Plan for minimal user impact
- **Backup Production Database**: Create full backup before migration
- **Apply Migration**: Run migration on production database
- **Verify Application**: Ensure application starts and functions correctly
- **Monitor Performance**: Watch for any performance issues
- **Rollback if Needed**: Execute rollback plan if issues arise

### 8. Post-Migration Tasks
- **Update Documentation**: Document schema changes
- **Clean Up Old Migrations**: Remove unnecessary migration files
- **Monitor Application**: Watch for any issues in the days following
- **Update Deployment Scripts**: Ensure future deployments include migration

## Migration Best Practices

### Safe Migration Patterns
```csharp
// 1. Add new column as nullable first
public string NewProperty { get; set; }

// 2. Populate data in new column
// 3. Make column non-nullable
[Required]
public string NewProperty { get; set; } = string.Empty;

// 4. Remove old column in separate migration
```

### Breaking Changes
- **Column Renames**: Use multiple migrations (add new, migrate data, remove old)
- **Type Changes**: Create new column, migrate data, drop old column
- **Constraint Changes**: Add constraints in separate migration after data migration

### Performance Considerations
- **Large Tables**: Use batch processing for data migrations
- **Indexes**: Add indexes after data migration to speed up process
- **Foreign Keys**: Temporarily disable constraints during migration if needed

## Common Migration Commands

```bash
# Create new migration
dotnet ef migrations add AddCustomerAppointments

# Update database
dotnet ef database update

# Remove last migration (if not applied)
dotnet ef migrations remove

# Generate SQL script
dotnet ef migrations script

# Generate SQL script for specific range
dotnet ef migrations script FromMigration ToMigration
```

## Quality Checklist
- [ ] Migration has been tested on development database
- [ ] Data migration scripts have been tested
- [ ] Rollback plan has been prepared and tested
- [ ] Performance impact has been assessed
- [ ] Application tests pass with new schema
- [ ] Staging deployment is successful
- [ ] Production backup has been created
- [ ] Migration has been reviewed by team
- [ ] Documentation has been updated
- [ ] Monitoring is in place for post-migration
